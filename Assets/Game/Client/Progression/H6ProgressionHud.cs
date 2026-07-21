using System.Text;
using Lumbre.Game.Domain.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Progression
{
    public sealed class H6ProgressionHud : MonoBehaviour
    {
        [SerializeField] private H6ProgressionRuntime runtime;
        [SerializeField] private Text label;

        public void Configure(H6ProgressionRuntime progressionRuntime, Text hudLabel)
        {
            runtime = progressionRuntime;
            label = hudLabel;
        }

        private void Awake()
        {
            runtime ??= FindFirstObjectByType<H6ProgressionRuntime>();
            label ??= GetComponent<Text>();
        }

        private void Update()
        {
            if (runtime?.Progression == null || label == null)
            {
                return;
            }

            var snapshot = runtime.Progression.Snapshot;
            var barLength = 20;
            var filled = Mathf.RoundToInt(snapshot.NormalizedProgress * barLength);
            var bar = new StringBuilder(barLength + 2);
            bar.Append('[');
            for (var index = 0; index < barLength; index++)
            {
                bar.Append(index < filled ? '#' : '-');
            }

            bar.Append(']');
            var suffix = snapshot.Level >= snapshot.MaxLevel ? "  MÁXIMO" : string.Empty;
            label.text = $"NIVEL {snapshot.Level}/{snapshot.MaxLevel}  "
                + $"XP {snapshot.TotalExperience}/{snapshot.ExperienceToNextLevel}  "
                + $"{bar}{suffix}";
        }
    }
}
