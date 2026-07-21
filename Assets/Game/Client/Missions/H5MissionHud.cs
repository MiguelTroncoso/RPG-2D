using System.Text;
using Lumbre.Game.Domain.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Missions
{
    public sealed class H5MissionHud : MonoBehaviour
    {
        [SerializeField] private H5MissionRuntime runtime;
        [SerializeField] private H5NaraController nara;
        [SerializeField] private Transform player;
        [SerializeField] private Text label;

        public void Configure(H5MissionRuntime missionRuntime, H5NaraController missionNpc,
            Transform playerTransform, Text hudLabel)
        {
            runtime = missionRuntime;
            nara = missionNpc;
            player = playerTransform;
            label = hudLabel;
        }

        private void Awake()
        {
            runtime ??= FindFirstObjectByType<H5MissionRuntime>();
            nara ??= FindFirstObjectByType<H5NaraController>();
            player ??= GameObject.FindWithTag("Player")?.transform;
            label ??= GetComponent<Text>();
        }

        private void Update()
        {
            if (runtime == null || runtime.Mission == null || label == null)
            {
                return;
            }

            var snapshot = runtime.Mission.Snapshot;
            var text = new StringBuilder();
            text.Append("NARA VELAQUIETA  |  ");
            text.Append(snapshot.State switch
            {
                MissionState.Available => "MISIÓN DISPONIBLE",
                MissionState.Active => "MISIÓN ACTIVA",
                MissionState.ReadyToTurnIn => "REGRESA CON NARA",
                MissionState.Completed => "MISIÓN COMPLETADA",
                _ => "MISIÓN"
            });
            text.Append('\n');
            text.Append($"Mordeluz {snapshot.MordeluzDefeated}/{snapshot.MordeluzRequired}  |  ");
            text.Append($"Resonante {snapshot.ResonantDefeated}/{snapshot.ResonantRequired}");
            text.Append('\n');
            text.Append("Inventario ");
            text.Append(runtime.Inventory.Count);
            text.Append('/');
            text.Append(runtime.Inventory.Capacity);
            text.Append(": ");
            for (var index = 0; index < runtime.Inventory.Capacity; index++)
            {
                var item = runtime.Inventory.GetAt(index);
                text.Append(item.HasValue ? $"[{index + 1}] {item.Value.DisplayName}" : "[—]");
                if (index < runtime.Inventory.Capacity - 1)
                {
                    text.Append("  ");
                }
            }

            text.Append('\n');
            text.Append("Ranura reliquia: ");
            text.Append(runtime.Equipment.EquippedItem.HasValue
                ? runtime.Equipment.EquippedItem.Value.DisplayName
                : "vacía");
            if (nara != null && player != null && nara.IsInRange(player))
            {
                text.Append("  |  F/HABLAR");
            }

            if (runtime.Inventory.Contains(H5MissionModel.RewardItemId)
                && !runtime.Equipment.EquippedItem.HasValue)
            {
                text.Append("  |  G/EQUIPAR");
            }

            label.text = text.ToString();
        }
    }
}
