using Lumbre.Game.Domain.Missions;
using UnityEngine;

namespace Lumbre.Game.Client.Missions
{
    public sealed class H5NaraController : MonoBehaviour
    {
        [SerializeField] private H5MissionRuntime runtime;
        [SerializeField, Min(0.1f)] private float interactionRange = 1.65f;

        public H5MissionRuntime Runtime => runtime;
        public float InteractionRange => interactionRange;

        public void Configure(H5MissionRuntime missionRuntime, float range)
        {
            runtime = missionRuntime;
            interactionRange = Mathf.Max(0.1f, range);
        }

        public bool IsInRange(Transform interactor)
        {
            return interactor != null
                && Vector2.Distance(transform.position, interactor.position) <= interactionRange;
        }

        public MissionOperationResult TryInteract(Transform interactor)
        {
            if (runtime == null || !IsInRange(interactor))
            {
                return MissionOperationResult.Failure(MissionOperationCode.NotAvailable);
            }

            if (runtime.Mission.State == MissionState.Available)
            {
                return runtime.Mission.TryAccept();
            }

            if (runtime.Mission.State == MissionState.ReadyToTurnIn)
            {
                return runtime.Mission.TryTurnIn();
            }

            return MissionOperationResult.Failure(
                runtime.Mission.State == MissionState.Completed
                    ? MissionOperationCode.Completed
                    : MissionOperationCode.AlreadyActive);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.75f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
