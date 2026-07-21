using System.Text;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Missions;
using Lumbre.Game.Client.Progression;
using Lumbre.Game.Domain.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Presentation
{
    public sealed class H7StatusHud : MonoBehaviour
    {
        [SerializeField] private H4CombatHealth health;
        [SerializeField] private H4BPlayerAbilityController abilities;
        [SerializeField] private H5MissionRuntime mission;
        [SerializeField] private H6ProgressionRuntime progression;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text missionLabel;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider heatBar;
        [SerializeField] private Slider experienceBar;
        [SerializeField] private RectTransform pulseTarget;

        private float _pulseRemaining;
        private float _refreshRemaining;

        public bool IsConfigured => health != null && abilities != null && mission != null
            && progression != null && statusLabel != null && missionLabel != null
            && healthBar != null && heatBar != null && experienceBar != null;

        public void Configure(H4CombatHealth playerHealth, H4BPlayerAbilityController playerAbilities,
            H5MissionRuntime missionRuntime, H6ProgressionRuntime progressionRuntime,
            Text status, Text missionText, Slider healthSlider, Slider heatSlider,
            Slider experienceSlider, RectTransform target)
        {
            health = playerHealth;
            abilities = playerAbilities;
            mission = missionRuntime;
            progression = progressionRuntime;
            statusLabel = status;
            missionLabel = missionText;
            healthBar = healthSlider;
            heatBar = heatSlider;
            experienceBar = experienceSlider;
            pulseTarget = target;
        }

        public void Pulse()
        {
            _pulseRemaining = 0.45f;
        }

        private void Awake()
        {
            health ??= FindFirstObjectByType<H4CombatHealth>();
            abilities ??= FindFirstObjectByType<H4BPlayerAbilityController>();
            mission ??= FindFirstObjectByType<H5MissionRuntime>();
            progression ??= FindFirstObjectByType<H6ProgressionRuntime>();
        }

        private void Update()
        {
            if (!IsConfigured)
            {
                return;
            }

            _refreshRemaining -= Time.unscaledDeltaTime;
            if (_refreshRemaining > 0f)
            {
                UpdatePulse();
                return;
            }

            _refreshRemaining = 0.1f;

            healthBar.value = health.MaxHealth <= 0 ? 0f : health.CurrentHealth / (float)health.MaxHealth;
            heatBar.value = abilities.MaxHeat <= 0 ? 0f : abilities.CurrentHeat / (float)abilities.MaxHeat;
            var experience = progression.Progression;
            experienceBar.value = experience == null ? 0f : experience.Snapshot.NormalizedProgress;

            statusLabel.text = $"BASTIÓN  ·  VIDA {health.CurrentHealth}/{health.MaxHealth}  ·  "
                + $"CALOR {abilities.CurrentHeat}/{abilities.MaxHeat}\n"
                + $"NIVEL {experience?.Snapshot.Level ?? 1}  ·  "
                + $"INVENTARIO {mission.Inventory.Count}/{mission.Inventory.Capacity}  ·  "
                + $"RELIQUIA {(mission.Equipment.EquippedItem.HasValue ? "EQUIPADA" : "VACÍA")}";

            var snapshot = mission.Mission.Snapshot;
            var progress = new StringBuilder();
            progress.Append("NARA VELAQUIETA  •  ");
            progress.Append(snapshot.State switch
            {
                MissionState.Available => "MISIÓN DISPONIBLE",
                MissionState.Active => "MISIÓN ACTIVA",
                MissionState.ReadyToTurnIn => "REGRESA CON NARA",
                MissionState.Completed => "MISIÓN COMPLETADA",
                _ => "MISIÓN"
            });
            progress.Append('\n');
            progress.Append($"MORDELUZ  {snapshot.MordeluzDefeated}/{snapshot.MordeluzRequired}   •   ");
            progress.Append($"RESONANTE  {snapshot.ResonantDefeated}/{snapshot.ResonantRequired}");
            missionLabel.text = progress.ToString();

            UpdatePulse();
        }

        private void UpdatePulse()
        {
            if (pulseTarget == null)
            {
                return;
            }

            if (_pulseRemaining > 0f)
            {
                _pulseRemaining -= Time.unscaledDeltaTime;
                pulseTarget.localScale = Vector3.one * (1f + _pulseRemaining * 0.18f);
            }
            else
            {
                pulseTarget.localScale = Vector3.one;
            }
        }
    }
}
