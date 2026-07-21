using Lumbre.Game.Client.Player;
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
        [SerializeField] private H3PlayerInputReader inputReader;

        public void Configure(H5MissionRuntime missionRuntime, H5NaraController missionNpc,
            Transform playerTransform, Text hudLabel, H3PlayerInputReader reader = null)
        {
            runtime = missionRuntime;
            nara = missionNpc;
            player = playerTransform;
            label = hudLabel;
            inputReader = reader;
        }

        private void Awake()
        {
            runtime ??= FindFirstObjectByType<H5MissionRuntime>();
            nara ??= FindFirstObjectByType<H5NaraController>();
            player ??= GameObject.FindWithTag("Player")?.transform;
            label ??= GetComponent<Text>();
            inputReader ??= player?.GetComponent<H3PlayerInputReader>();
        }

        private void Update()
        {
            if (runtime == null || runtime.Mission == null || label == null)
            {
                return;
            }

            var snapshot = runtime.Mission.Snapshot;
            var text = snapshot.State switch
            {
                MissionState.Available => "MISIÓN DISPONIBLE",
                MissionState.Active => "MISIÓN ACTIVA",
                MissionState.ReadyToTurnIn => "REGRESA CON NARA",
                MissionState.Completed => "MISIÓN COMPLETADA",
                _ => "MISIÓN"
            };

            var prompt = string.Empty;
            var touchPrompt = inputReader != null && inputReader.IsTouchControlSchemeActive;
            if (nara != null && player != null && nara.IsInRange(player))
            {
                prompt = touchPrompt
                    ? "  ·  HABLAR CON NARA"
                    : "  ·  F / HABLAR CON NARA";
            }

            if (runtime.Inventory.Contains(H5MissionModel.RewardItemId)
                && !runtime.Equipment.EquippedItem.HasValue)
            {
                prompt += touchPrompt
                    ? "  ·  EQUIPAR RECOMPENSA"
                    : "  ·  G / EQUIPAR RECOMPENSA";
            }

            label.text = prompt.Length == 0 ? string.Empty : text + prompt;
        }
    }
}
