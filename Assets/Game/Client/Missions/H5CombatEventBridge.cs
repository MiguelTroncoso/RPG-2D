using Lumbre.Game.Client.Combat;
using Lumbre.Game.Domain.Events;
using UnityEngine;

namespace Lumbre.Game.Client.Missions
{
    public sealed class H5CombatEventBridge : MonoBehaviour
    {
        [SerializeField] private H5MissionRuntime runtime;
        [SerializeField] private CombatantKind combatantKind;

        private H4CombatHealth _health;

        public CombatantKind CombatantKind => combatantKind;

        public void Configure(H5MissionRuntime missionRuntime, CombatantKind kind)
        {
            runtime = missionRuntime;
            combatantKind = kind;
        }

        private void Awake()
        {
            _health = GetComponent<H4CombatHealth>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.Died += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Died -= HandleDeath;
            }
        }

        private void HandleDeath()
        {
            runtime?.Publish(new CombatantDefeatedEvent(combatantKind, gameObject.name));
        }
    }
}
