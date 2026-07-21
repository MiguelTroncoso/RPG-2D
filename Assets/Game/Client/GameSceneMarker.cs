using UnityEngine;

namespace Lumbre.Game.Client
{
    public sealed class GameSceneMarker : MonoBehaviour
    {
        [SerializeField]
        private string marker;

        public string Marker
        {
            get => marker;
            set => marker = value;
        }
    }
}
