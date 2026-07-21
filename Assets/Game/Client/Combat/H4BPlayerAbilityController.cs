using System;
using System.Collections.Generic;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Combat;
using Lumbre.Game.Domain.Constants;
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

        private ICombatTimeSource _timeSource;
        private HeatResourceModel _heat;
        private DefenseAbilityModel _defense;
        private AreaAttackAbilityModel _areaAttack;

        public int CurrentHeat => _heat?.CurrentHeat ?? Mathf.Clamp(initialHeat, 0, MaxHeat);
        public int MaxHeat => Mathf.Max(1, maxHeat);
        public bool IsDefenseActive => _defense != null && _defense.IsActive;
        public float DefenseRemaining => _defense?.RemainingDuration ?? 0f;
        public float DefenseCooldownRemaining => _defense?.CooldownRemaining ?? 0f;
        public int AreaHeatCost => _areaAttack?.HeatCost ?? Mathf.Max(0, areaHeatCost);
        public float AreaCooldownRemaining => _areaAttack?.CooldownRemaining ?? 0f;
        public event Action<AbilityResult> DefenseActivated;
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
            if (inputReader == null || health != null && !health.IsAlive)
            {
                return;
            }

            if (inputReader.DefensePressedThisFrame)
            {
                TryActivateDefense();
            }

            if (inputReader.AreaAttackPressedThisFrame)
            {
                TryAreaAttack();
            }
        }

        public AbilityResult TryActivateDefense()
        {
            InitializeModelsIfNeeded();
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
            InitializeModelsIfNeeded();
            var result = _areaAttack.TryActivate();
            if (!result.Succeeded)
            {
                feedback?.PlayRejected(result.Code);
                return result;
            }

            var targets = new HashSet<H4CombatHealth>();
            var colliders = Physics2D.OverlapCircleAll(transform.position, areaRadius, targetLayers);
            foreach (var collider in colliders)
            {
                var target = collider.GetComponentInParent<H4CombatHealth>();
                if (target == null || target == health || !target.IsTargetable || !targets.Add(target))
                {
                    continue;
                }

                target.ReceiveDamage(new CombatDamage(areaDamage, "player-heat-area"));
            }

            feedback?.PlayArea(areaRadius);
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
            Gizmos.DrawWireSphere(transform.position, areaRadius);
        }
    }
}
