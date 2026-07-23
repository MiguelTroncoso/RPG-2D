using System;
using System.Collections;
using System.Collections.Generic;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Combat;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Movement;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class H4BPlayerAbilityController : MonoBehaviour
    {
        [SerializeField] private H3PlayerInputReader inputReader;
        [SerializeField] private H4PlayerCombatController combatController;
        [SerializeField] private H4CombatHealth health;
        [SerializeField] private H4BAbilityFeedback feedback;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField, Min(1)] private int maxHeat = ProjectConstants.PlayerHeatMax;
        [SerializeField, Min(0)] private int initialHeat = ProjectConstants.PlayerHeatInitial;
        [SerializeField, Min(1)] private int heatPerBasicAttack = ProjectConstants.PlayerHeatPerBasicAttack;
        [SerializeField, Min(0f)] private float defenseDuration = ProjectConstants.PlayerDefenseDuration;
        [SerializeField, Min(0f)] private float defenseCooldown = ProjectConstants.PlayerDefenseCooldown;
        [SerializeField, Range(0f, 0.95f)] private float defenseDamageReduction =
            ProjectConstants.PlayerDefenseDamageReduction;
        [SerializeField, Min(0)] private int areaHeatCost = ProjectConstants.PlayerAreaAttackHeatCost;
        [SerializeField, Min(1)] private int areaDamage = ProjectConstants.PlayerAreaAttackDamage;
        [SerializeField, Min(0.1f)] private float areaRadius = ProjectConstants.PlayerAreaAttackRadius;
        [SerializeField, Min(0f)] private float areaCooldown = ProjectConstants.PlayerAreaAttackCooldown;
        private float areaAnticipationDuration = 0.14f;
        private float areaRecoveryDuration = 0.12f;

        private ICombatTimeSource _timeSource;
        private HeatResourceModel _heat;
        private DefenseAbilityModel _defense;
        private AreaAttackAbilityModel _areaAttack;
        [NonSerialized] private H10PlayerActionStateController _actionState;
        [NonSerialized] private Coroutine _areaRoutine;

        public int CurrentHeat => _heat?.CurrentHeat ?? Mathf.Clamp(initialHeat, 0, MaxHeat);
        public int MaxHeat => Mathf.Max(1, maxHeat);
        public bool IsDefenseActive => _defense != null && _defense.IsActive;
        public float DefenseRemaining => _defense?.RemainingDuration ?? 0f;
        public float DefenseCooldownRemaining => _defense?.CooldownRemaining ?? 0f;
        public int AreaHeatCost => _areaAttack?.HeatCost ?? Mathf.Max(0, areaHeatCost);
        public float AreaCooldownRemaining => _areaAttack?.CooldownRemaining ?? 0f;
        public float AreaRadius => _areaAttack?.Radius ?? Mathf.Max(0.1f, areaRadius);
        public float AreaAnticipationDuration => Mathf.Max(0f, areaAnticipationDuration);
        public float AreaRecoveryDuration => Mathf.Max(0f, areaRecoveryDuration);
        public event Action<AbilityResult> DefenseActivated;
        public event Action AreaAttackSequenceStarted;
        public event Action<AbilityResult> AreaAttackActivated;

        public void Configure(H3PlayerInputReader reader,
            H4PlayerCombatController playerCombat,
            H4CombatHealth combatHealth,
            H4BAbilityFeedback abilityFeedback,
            LayerMask layers)
        {
            inputReader = reader;
            combatController = playerCombat;
            health = combatHealth;
            feedback = abilityFeedback;
            targetLayers = layers;
            if (UnityEngine.Application.isPlaying)
            {
                InitializeModels(_timeSource ?? new UnityCombatTimeSource());
            }
        }

        public void SetTimeSource(ICombatTimeSource timeSource)
        {
            _timeSource = timeSource ?? new UnityCombatTimeSource();
            InitializeModels(_timeSource);
        }

        private void Awake()
        {
            inputReader ??= GetComponent<H3PlayerInputReader>();
            combatController ??= GetComponent<H4PlayerCombatController>();
            health ??= GetComponent<H4CombatHealth>();
            feedback ??= GetComponent<H4BAbilityFeedback>();
            _actionState = GetComponent<H10PlayerActionStateController>();
            InitializeModels(_timeSource ?? new UnityCombatTimeSource());
        }

        private void OnEnable()
        {
            if (combatController != null)
            {
                combatController.BasicAttackSucceeded += HandleBasicAttack;
            }
        }

        private void OnDisable()
        {
            if (combatController != null)
            {
                combatController.BasicAttackSucceeded -= HandleBasicAttack;
            }

            if (_areaRoutine != null)
            {
                StopCoroutine(_areaRoutine);
                _areaRoutine = null;
            }

            _actionState?.Complete(PlayerActionState.AreaAttack);
        }

        private void OnDestroy()
        {
            if (health != null && health.DamageModifier == _defense)
            {
                health.DamageModifier = null;
            }
        }

        private void Update()
        {
            if (inputReader == null || Time.timeScale <= 0f || health != null && !health.IsAlive)
            {
                return;
            }

            if (inputReader.DefensePressedThisFrame)
            {
                TryActivateDefenseFromInput();
                return;
            }

            if (inputReader.AreaAttackPressedThisFrame)
            {
                TryAreaAttackFromInput();
            }
        }

        public AbilityResult TryActivateDefense()
        {
            return TryActivateDefenseCore();
        }

        private void TryActivateDefenseFromInput()
        {
            if (_actionState != null && !_actionState.TryBegin(PlayerActionState.Defending))
            {
                return;
            }

            try
            {
                TryActivateDefenseCore();
            }
            finally
            {
                _actionState?.Complete(PlayerActionState.Defending);
            }
        }

        private AbilityResult TryActivateDefenseCore()
        {
            InitializeModelsIfNeeded();
            if (Time.timeScale <= 0f || health != null && !health.IsAlive)
            {
                var blocked = AbilityResult.Failure(AbilityResultCode.InvalidState);
                feedback?.PlayRejected(blocked.Code);
                return blocked;
            }

            var result = _defense.TryActivate();
            if (result.Succeeded)
            {
                feedback?.PlayDefense(defenseDuration);
                DefenseActivated?.Invoke(result);
            }
            else
            {
                feedback?.PlayRejected(result.Code);
            }

            return result;
        }

        public AbilityResult TryAreaAttack()
        {
            return TryAreaAttackCore();
        }

        public void ConfigurePresentation(float anticipationDuration, float recoveryDuration)
        {
            areaAnticipationDuration = Mathf.Max(0f, anticipationDuration);
            areaRecoveryDuration = Mathf.Max(0f, recoveryDuration);
        }

        private void TryAreaAttackFromInput()
        {
            if (_areaRoutine != null || _actionState != null
                && !_actionState.TryBegin(PlayerActionState.AreaAttack))
            {
                return;
            }

            InitializeModelsIfNeeded();
            if (Time.timeScale <= 0f || health != null && !health.IsAlive
                || _areaAttack == null || !_areaAttack.CanActivate)
            {
                TryAreaAttackCore();
                _actionState?.Complete(PlayerActionState.AreaAttack);
                return;
            }

            _areaRoutine = StartCoroutine(AreaAttackSequence());
        }

        private IEnumerator AreaAttackSequence()
        {
            try
            {
                AreaAttackSequenceStarted?.Invoke();
                feedback?.PlayAreaPreview(AreaRadius, AreaAnticipationDuration);
                yield return new WaitForSeconds(AreaAnticipationDuration);
                TryAreaAttackCore();
                yield return new WaitForSeconds(AreaRecoveryDuration);
            }
            finally
            {
                _areaRoutine = null;
                _actionState?.Complete(PlayerActionState.AreaAttack);
            }
        }

        private AbilityResult TryAreaAttackCore()
        {
            InitializeModelsIfNeeded();
            if (Time.timeScale <= 0f || health != null && !health.IsAlive)
            {
                var blocked = AbilityResult.Failure(AbilityResultCode.InvalidState);
                feedback?.PlayRejected(blocked.Code);
                return blocked;
            }

            var result = _areaAttack.TryActivate();
            if (!result.Succeeded)
            {
                feedback?.PlayRejected(result.Code);
                return result;
            }

            var effectiveRadius = _areaAttack.Radius;
            var targets = new HashSet<H4CombatHealth>();
            var colliders = Physics2D.OverlapCircleAll(transform.position, effectiveRadius, targetLayers);
            foreach (var collider in colliders)
            {
                var target = collider.GetComponentInParent<H4CombatHealth>();
                if (target == null || target == health || !target.IsTargetable || !targets.Add(target))
                {
                    continue;
                }

                target.ReceiveDamage(new CombatDamage(areaDamage, "player-heat-area"));
            }

            feedback?.PlayArea(effectiveRadius);
            AreaAttackActivated?.Invoke(result);
            return result;
        }

        public int AddHeat(int amount)
        {
            InitializeModelsIfNeeded();
            var value = _heat.Add(amount);
            feedback?.PlayHeatGain(amount);
            return value;
        }

        private void HandleBasicAttack(AttackResult result)
        {
            if (result.Succeeded)
            {
                AddHeat(heatPerBasicAttack);
            }
        }

        private void InitializeModelsIfNeeded()
        {
            if (_heat == null || _defense == null || _areaAttack == null)
            {
                InitializeModels(_timeSource ?? new UnityCombatTimeSource());
            }
        }

        private void InitializeModels(ICombatTimeSource timeSource)
        {
            _timeSource = timeSource;
            _heat = new HeatResourceModel(MaxHeat, initialHeat);
            _defense = new DefenseAbilityModel(
                _timeSource,
                defenseDuration,
                defenseCooldown,
                defenseDamageReduction);
            _areaAttack = new AreaAttackAbilityModel(
                _timeSource,
                _heat,
                areaHeatCost,
                areaDamage,
                areaRadius,
                areaCooldown);
            if (health != null)
            {
                health.DamageModifier = _defense;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.9f, 0.65f);
            Gizmos.DrawWireSphere(transform.position, AreaRadius);
        }

    }
}
