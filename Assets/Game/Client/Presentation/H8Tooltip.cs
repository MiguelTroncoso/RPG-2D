using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Presentation
{
    public sealed class H8Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler
    {
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private Text label;
        [SerializeField] private string message;

        public bool IsConfigured => panel != null && label != null && !string.IsNullOrWhiteSpace(message);

        public void Configure(CanvasGroup targetPanel, Text targetLabel, string tooltipMessage)
        {
            panel = targetPanel;
            label = targetLabel;
            message = tooltipMessage;
            label.text = message;
            SetVisible(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetVisible(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetVisible(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetVisible(true);
        }

        private void Awake()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (panel == null)
            {
                return;
            }

            panel.alpha = visible ? 1f : 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
    }
}
