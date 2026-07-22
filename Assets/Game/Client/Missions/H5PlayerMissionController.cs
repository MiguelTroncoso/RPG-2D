using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Inventory;
using Lumbre.Game.Domain.Missions;
using Lumbre.Game.Domain.Movement;
using UnityEngine;

namespace Lumbre.Game.Client.Missions
{
    public sealed class H5PlayerMissionController : MonoBehaviour
    {
        [SerializeField] private H3PlayerInputReader inputReader;
        [SerializeField] private H5MissionRuntime runtime;
        [SerializeField] private H5NaraController nara;
        private H10PlayerActionStateController actionState;

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
            actionState ??= GetComponent<H10PlayerActionStateController>();
        }

        private void Update()
        {
            if (inputReader == null || Time.timeScale <= 0f)
            {
                return;
            }

            if (inputReader.InteractPressedThisFrame)
            {
                TryInteractFromInput();
                return;
            }

            if (inputReader.EquipPressedThisFrame)
            {
                TryEquipFromInput();
            }
        }

        private void TryInteractFromInput()
        {
            if (actionState != null && !actionState.TryBegin(PlayerActionState.Interacting))
            {
                return;
            }

            try
            {
                TryInteract();
            }
            finally
            {
                actionState?.Complete(PlayerActionState.Interacting);
            }
        }

        private void TryEquipFromInput()
        {
            if (actionState != null && !actionState.TryBegin(PlayerActionState.Interacting))
            {
                return;
            }

            try
            {
                TryEquipReward();
            }
            finally
            {
                actionState?.Complete(PlayerActionState.Interacting);
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
