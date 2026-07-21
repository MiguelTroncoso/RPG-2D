using System.Collections.Generic;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Missions;
using Lumbre.Game.Client.Progression;
using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Missions;
using Lumbre.Game.Domain.Progression;
using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    [DefaultExecutionOrder(-50)]
    public sealed class H7PresentationRuntime : MonoBehaviour
    {
        [SerializeField] private H7VfxPool vfxPool;
        [SerializeField] private H7AudioFeedback audioFeedback;
        [SerializeField] private H7CameraPolish cameraPolish;
        [SerializeField] private H7StatusHud statusHud;
        [SerializeField] private H5MissionRuntime missionRuntime;
        [SerializeField] private H6ProgressionRuntime progressionRuntime;
        [SerializeField] private H4PlayerCombatController playerCombat;
        [SerializeField] private H4BPlayerAbilityController playerAbilities;
        [SerializeField] private H4CombatHealth playerHealth;

        private readonly List<H4CombatHealth> _healthSubscriptions = new List<H4CombatHealth>();
        private readonly Dictionary<H4CombatHealth, System.Action<Lumbre.Game.Domain.Combat.DamageResult>>
            _damageHandlers = new Dictionary<H4CombatHealth, System.Action<Lumbre.Game.Domain.Combat.DamageResult>>();
        private readonly Dictionary<H4CombatHealth, System.Action> _deathHandlers =
            new Dictionary<H4CombatHealth, System.Action>();
        private string _lastEquippedItem;
        private bool _initialized;

        public H7VfxPool VfxPool => vfxPool;
        public H7AudioFeedback AudioFeedback => audioFeedback;
        public H7CameraPolish CameraPolish => cameraPolish;
        public bool IsConfigured => vfxPool != null && audioFeedback != null
            && cameraPolish != null && statusHud != null;

        public void Configure(H7VfxPool pool, H7AudioFeedback audio, H7CameraPolish camera,
            H7StatusHud hud, H5MissionRuntime mission, H6ProgressionRuntime progression,
            H4PlayerCombatController combat, H4BPlayerAbilityController abilities,
            H4CombatHealth health)
        {
            vfxPool = pool;
            audioFeedback = audio;
            cameraPolish = camera;
            statusHud = hud;
            missionRuntime = mission;
            progressionRuntime = progression;
            playerCombat = combat;
            playerAbilities = abilities;
            playerHealth = health;
        }

        private void Awake()
        {
            vfxPool ??= GetComponent<H7VfxPool>();
            audioFeedback ??= GetComponent<H7AudioFeedback>();
            cameraPolish ??= FindFirstObjectByType<H7CameraPolish>();
            statusHud ??= FindFirstObjectByType<H7StatusHud>();
            missionRuntime ??= FindFirstObjectByType<H5MissionRuntime>();
            progressionRuntime ??= FindFirstObjectByType<H6ProgressionRuntime>();
            playerCombat ??= FindFirstObjectByType<H4PlayerCombatController>();
            playerAbilities ??= FindFirstObjectByType<H4BPlayerAbilityController>();
            playerHealth ??= GameObject.FindWithTag("Player")?.GetComponent<H4CombatHealth>();
        }

        private void Start()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            SubscribePlayer();
            SubscribeCombatants();
            if (missionRuntime?.EventBus != null)
            {
                missionRuntime.EventBus.Published += HandleDomainEvent;
            }

            _lastEquippedItem = missionRuntime?.Equipment?.EquippedItemId ?? string.Empty;
        }

        private void Update()
        {
            if (missionRuntime?.Equipment == null)
            {
                return;
            }

            var equipped = missionRuntime.Equipment.EquippedItemId ?? string.Empty;
            if (!string.Equals(equipped, _lastEquippedItem, System.StringComparison.Ordinal))
            {
                _lastEquippedItem = equipped;
                if (!string.IsNullOrWhiteSpace(equipped))
                {
                    vfxPool?.Play(H7VfxKind.Equip, GameObject.FindWithTag("Player")?.transform.position
                        ?? transform.position, new Color(0.55f, 0.9f, 1f, 1f));
                    audioFeedback?.PlayEquip();
                    statusHud?.Pulse();
                }
            }
        }

        private void SubscribePlayer()
        {
            if (playerCombat != null)
            {
                playerCombat.BasicAttackSucceeded += HandleBasicAttack;
            }

            if (playerAbilities != null)
            {
                playerAbilities.DefenseActivated += HandleDefense;
                playerAbilities.AreaAttackActivated += HandleAreaAttack;
            }
        }

        private void SubscribeCombatants()
        {
            var combatants = FindObjectsByType<H4CombatHealth>(FindObjectsSortMode.None);
            for (var index = 0; index < combatants.Length; index++)
            {
                if (_healthSubscriptions.Contains(combatants[index]))
                {
                    continue;
                }

                var combatant = combatants[index];
                _healthSubscriptions.Add(combatant);
                System.Action<Lumbre.Game.Domain.Combat.DamageResult> damageHandler =
                    result => HandleDamage(combatant, result);
                System.Action deathHandler = () => HandleDeath(combatant);
                _damageHandlers[combatant] = damageHandler;
                _deathHandlers[combatant] = deathHandler;
                combatant.DamageTaken += damageHandler;
                combatant.Died += deathHandler;
            }
        }

        private void HandleBasicAttack(Lumbre.Game.Domain.Combat.AttackResult result)
        {
            if (!result.Succeeded)
            {
                return;
            }

            var position = GameObject.FindWithTag("Player")?.transform.position ?? transform.position;
            vfxPool?.Play(H7VfxKind.Attack, position, new Color(1f, 0.55f, 0.18f, 1f));
            audioFeedback?.PlayAttack();
            cameraPolish?.Shake(0.06f);
        }

        private void HandleDefense(Lumbre.Game.Domain.Combat.AbilityResult result)
        {
            if (!result.Succeeded)
            {
                return;
            }

            var position = GameObject.FindWithTag("Player")?.transform.position ?? transform.position;
            vfxPool?.Play(H7VfxKind.Defense, position, new Color(0.25f, 0.85f, 1f, 1f));
            cameraPolish?.Shake(0.025f);
        }

        private void HandleAreaAttack(Lumbre.Game.Domain.Combat.AbilityResult result)
        {
            if (!result.Succeeded)
            {
                return;
            }

            var position = GameObject.FindWithTag("Player")?.transform.position ?? transform.position;
            vfxPool?.Play(H7VfxKind.Area, position, new Color(1f, 0.28f, 0.8f, 1f));
            cameraPolish?.Shake(0.1f);
        }

        private void HandleDamage(H4CombatHealth combatant,
            Lumbre.Game.Domain.Combat.DamageResult result)
        {
            if (result.Applied && !result.Killed)
            {
                vfxPool?.Play(H7VfxKind.Impact, combatant.transform.position,
                    new Color(1f, 0.35f, 0.25f, 1f));
                audioFeedback?.PlayImpact();
            }
        }

        private void HandleDeath(H4CombatHealth combatant)
        {
            vfxPool?.Play(H7VfxKind.Death, combatant.transform.position,
                new Color(0.55f, 0.25f, 0.8f, 1f));
            audioFeedback?.PlayDeath();
            cameraPolish?.Shake(0.08f);
        }

        private void HandleDomainEvent(IDomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case MissionStateChangedEvent stateChanged when stateChanged.Current
                    == Lumbre.Game.Domain.Missions.MissionState.Active:
                    vfxPool?.Play(H7VfxKind.Mission, transform.position,
                        new Color(0.25f, 0.95f, 0.85f, 1f));
                    audioFeedback?.PlayMission();
                    statusHud?.Pulse();
                    break;
                case MissionRewardGrantedEvent:
                    vfxPool?.Play(H7VfxKind.Mission, transform.position,
                        new Color(1f, 0.75f, 0.25f, 1f));
                    audioFeedback?.PlayMission();
                    statusHud?.Pulse();
                    break;
                case LevelUpEvent:
                    vfxPool?.Play(H7VfxKind.LevelUp, transform.position,
                        new Color(1f, 0.9f, 0.3f, 1f));
                    audioFeedback?.PlayLevelUp();
                    cameraPolish?.PlayLevelUpZoom();
                    statusHud?.Pulse();
                    break;
            }
        }

        private void OnDestroy()
        {
            if (playerCombat != null)
            {
                playerCombat.BasicAttackSucceeded -= HandleBasicAttack;
            }

            if (playerAbilities != null)
            {
                playerAbilities.DefenseActivated -= HandleDefense;
                playerAbilities.AreaAttackActivated -= HandleAreaAttack;
            }

            if (missionRuntime?.EventBus != null)
            {
                missionRuntime.EventBus.Published -= HandleDomainEvent;
            }

            for (var index = 0; index < _healthSubscriptions.Count; index++)
            {
                var combatant = _healthSubscriptions[index];
                if (_damageHandlers.TryGetValue(combatant, out var damageHandler))
                {
                    combatant.DamageTaken -= damageHandler;
                }

                if (_deathHandlers.TryGetValue(combatant, out var deathHandler))
                {
                    combatant.Died -= deathHandler;
                }
            }
        }
    }
}
