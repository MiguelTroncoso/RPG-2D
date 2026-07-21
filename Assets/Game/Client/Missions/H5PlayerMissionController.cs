using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Inventory;
using Lumbre.Game.Domain.Missions;
using UnityEngine;

namespace Lumbre.Game.Client.Missions
{
    public sealed class H5PlayerMissionController : MonoBehaviour
    {
        [SerializeField] private H3PlayerInputReader inputReader;
        [SerializeField] private H5MissionRuntime runtime;
        [SerializeField] private H5NaraController nara;

        public MissionOperationResult LastInteractionResult { get; private set; }
        public EquipmentOperationResult LastEquipmentResult { get; private set; }

        public void Configure(H3PlayerInputReader reader, H5MissionRuntime missionRuntime,
            H5NaraController missionNpc)
        {
            inputReader = reader;
            runtime = missionRuntime;
            nara = missionNpc;
        }

        private void Awake()
        {
            inputReader ??= GetComponent<H3PlayerInputReader>();
            runtime ??= FindFirstObjectByType<H5MissionRuntime>();
            nara ??= FindFirstObjectByType<H5NaraController>();
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            if (inputReader.InteractPressedThisFrame)
            {
                TryInteract();
            }

            if (inputReader.EquipPressedThisFrame)
            {
                TryEquipReward();
            }
        }

        public MissionOperationResult TryInteract()
        {
            LastInteractionResult = nara != null
                ? nara.TryInteract(transform)
                : MissionOperationResult.Failure(MissionOperationCode.NotAvailable);
            return LastInteractionResult;
        }

        public EquipmentOperationResult TryEquipReward()
        {
            LastEquipmentResult = runtime != null
                ? runtime.TryEquipReward()
                : EquipmentOperationResult.Failure(EquipmentOperationCode.MissingItem);
            return LastEquipmentResult;
        }
    }
}
