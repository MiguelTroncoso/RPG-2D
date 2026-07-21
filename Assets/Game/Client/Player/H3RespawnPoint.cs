using UnityEngine;

namespace Lumbre.Game.Client.Player
{
    public sealed class H3RespawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.85f, 1f);
            Gizmos.DrawWireSphere(transform.position, 0.18f);
            Gizmos.DrawLine(transform.position + Vector3.left * 0.3f, transform.position + Vector3.right * 0.3f);
            Gizmos.DrawLine(transform.position + Vector3.down * 0.3f, transform.position + Vector3.up * 0.3f);
        }
    }
}
