using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class H4BWaveTelegraph : MonoBehaviour
    {
        [SerializeField] private Color dangerColor = new Color(1f, 0.65f, 0.1f, 1f);

        private LineRenderer _line;
        private float _radius;
        private bool _visible;

        public bool IsVisible => _visible;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            if (_line == null)
            {
                _line = gameObject.AddComponent<LineRenderer>();
            }

            _line.useWorldSpace = true;
            _line.loop = true;
            _line.positionCount = 40;
            _line.startWidth = 0.075f;
            _line.endWidth = 0.075f;
            _line.startColor = dangerColor;
            _line.endColor = dangerColor;
            _line.sortingOrder = 25;
            _line.enabled = false;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _line.material = new Material(shader);
            }
        }

        private void Update()
        {
            if (_visible)
            {
                DrawCircle();
            }
        }

        public void Show(float radius)
        {
            _radius = Mathf.Max(0.1f, radius);
            _visible = true;
            _line.enabled = true;
            DrawCircle();
        }

        public void Hide()
        {
            _visible = false;
            if (_line != null)
            {
                _line.enabled = false;
            }
        }

        private void DrawCircle()
        {
            var center = transform.position + Vector3.forward * 0.12f;
            for (var index = 0; index < _line.positionCount; index++)
            {
                var angle = index / (float)_line.positionCount * Mathf.PI * 2f;
                _line.SetPosition(index, center + new Vector3(
                    Mathf.Cos(angle) * _radius,
                    Mathf.Sin(angle) * _radius,
                    0f));
            }
        }

        private void OnDisable()
        {
            Hide();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = dangerColor;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
