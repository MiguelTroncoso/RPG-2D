using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    /// <summary>
    /// Keeps the presentation root inside the platform safe area without
    /// coupling gameplay or input code to a particular device shape.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class H9SafeAreaLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField, Min(0f)] private float padding = 18f;

        private Rect _lastSafeArea;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        public bool IsConfigured => target != null;
        public Rect AppliedSafeArea { get; private set; }
        public Vector2 LastScreenSize => new Vector2(_lastScreenWidth, _lastScreenHeight);

        public void Configure(RectTransform layoutTarget, float minimumPadding)
        {
            target = layoutTarget != null ? layoutTarget : GetComponent<RectTransform>();
            padding = Mathf.Max(0f, minimumPadding);
            ApplyNow();
        }

        private void Awake()
        {
            target ??= GetComponent<RectTransform>();
            ApplyNow();
        }

        private void OnEnable()
        {
            ApplyNow();
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            var currentSafeArea = GetSafeArea();
            if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height
                || _lastSafeArea != currentSafeArea)
            {
                ApplyNow();
            }
        }

        public void ApplyNow()
        {
            if (target == null)
            {
                return;
            }

            var width = Mathf.Max(1, Screen.width);
            var height = Mathf.Max(1, Screen.height);
            var safeArea = GetSafeArea();
            var min = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
            var max = new Vector2(safeArea.xMax / width, safeArea.yMax / height);

            target.anchorMin = min;
            target.anchorMax = max;
            target.offsetMin = new Vector2(padding, padding);
            target.offsetMax = new Vector2(-padding, -padding);

            _lastSafeArea = safeArea;
            _lastScreenWidth = width;
            _lastScreenHeight = height;
            AppliedSafeArea = safeArea;
        }

        private static Rect GetSafeArea()
        {
            var width = Mathf.Max(1, Screen.width);
            var height = Mathf.Max(1, Screen.height);
            var safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                safeArea = new Rect(0f, 0f, width, height);
            }

            return safeArea;
        }
    }
}
