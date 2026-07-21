using Unity.Cinemachine;
using UnityEngine;

namespace Lumbre.Game.Client.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class H3CinemachineFollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 0f, -20f);

        private CinemachineCamera _camera;
        private CinemachineFollow _follow;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Awake()
        {
            _camera = GetComponent<CinemachineCamera>();
            _follow = GetComponent<CinemachineFollow>() ?? gameObject.AddComponent<CinemachineFollow>();
            _follow.FollowOffset = followOffset;
            ResolveTarget();
        }

        private void LateUpdate()
        {
            ResolveTarget();
        }

        private void ResolveTarget()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target != null && _camera != null && _camera.Follow != target)
            {
                _camera.Follow = target;
            }
        }
    }
}
