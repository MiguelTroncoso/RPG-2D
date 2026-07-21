using Unity.Cinemachine;
using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class H7CameraPolish : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cmCamera;
        [SerializeField] private CinemachineFollow follow;
        [SerializeField, Min(0.01f)] private float shakeDuration = 0.12f;
        [SerializeField, Min(0.01f)] private float baseZoom = 9f;

        private Vector3 _baseOffset;
        private float _shakeRemaining;
        private float _shakeMagnitude;
        private float _zoomRemaining;
        private float _zoomProgress;

        public bool IsConfigured => cmCamera != null && follow != null;
        public bool IsShaking => _shakeRemaining > 0f;
        public bool IsZooming => _zoomRemaining > 0f;

        public void Configure(CinemachineCamera targetCamera, CinemachineFollow targetFollow)
        {
            cmCamera = targetCamera;
            follow = targetFollow;
            if (follow != null)
            {
                _baseOffset = follow.FollowOffset;
            }
        }

        private void Awake()
        {
            cmCamera ??= GetComponent<CinemachineCamera>();
            follow ??= GetComponent<CinemachineFollow>();
            if (follow != null)
            {
                _baseOffset = follow.FollowOffset;
            }

            if (cmCamera != null)
            {
                baseZoom = cmCamera.Lens.OrthographicSize;
            }
        }

        private void LateUpdate()
        {
            if (follow != null)
            {
                var offset = _baseOffset;
                if (_shakeRemaining > 0f)
                {
                    var phase = Time.time * 80f;
                    offset.x += Mathf.Sin(phase) * _shakeMagnitude;
                    offset.y += Mathf.Cos(phase * 1.13f) * _shakeMagnitude * 0.65f;
                    _shakeRemaining -= Time.deltaTime;
                }

                follow.FollowOffset = offset;
            }

            if (cmCamera != null && _zoomRemaining > 0f)
            {
                _zoomRemaining -= Time.deltaTime;
                _zoomProgress = Mathf.Clamp01(_zoomProgress + Time.deltaTime * 3.2f);
                var pulse = Mathf.Sin(_zoomProgress * Mathf.PI);
                var lens = cmCamera.Lens;
                lens.OrthographicSize = baseZoom - pulse * 0.45f;
                cmCamera.Lens = lens;
            }
        }

        public void Shake(float magnitude)
        {
            _shakeMagnitude = Mathf.Max(_shakeMagnitude, magnitude);
            _shakeRemaining = Mathf.Max(_shakeRemaining, shakeDuration);
        }

        public void PlayLevelUpZoom()
        {
            _zoomProgress = 0f;
            _zoomRemaining = 0.75f;
        }
    }
}
