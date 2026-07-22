using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Presentation
{
    /// <summary>
    /// Presentation-only pressed feedback for OnScreenButton controls. The
    /// actual action remains owned by Unity Input System's OnScreenButton.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class H10TouchButtonFeedback : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField, Range(0f, 1f)] private float pressedAlpha = 1f;
        [SerializeField, Range(0.8f, 1f)] private float pressedScale = 0.94f;

        private Image _image;
        private Color _normalColor;
        private Vector3 _normalScale;

        public bool IsPressed { get; private set; }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _normalColor = _image.color;
            _normalScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
            _image.color = new Color(_normalColor.r, _normalColor.g, _normalColor.b,
                Mathf.Clamp01(pressedAlpha));
            transform.localScale = _normalScale * Mathf.Clamp(pressedScale, 0.8f, 1f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetVisualState();
        }

        private void OnDisable()
        {
            ResetVisualState();
        }

        private void ResetVisualState()
        {
            IsPressed = false;
            if (_image != null)
            {
                _image.color = _normalColor;
            }

            transform.localScale = _normalScale;
        }
    }
}
