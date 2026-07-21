using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    public sealed class H7WorldArtLayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer backdrop;

        public SpriteRenderer Backdrop => backdrop;
        public bool IsConfigured => backdrop != null && backdrop.sprite != null;

        public void Configure(SpriteRenderer renderer)
        {
            backdrop = renderer;
        }
    }
}
