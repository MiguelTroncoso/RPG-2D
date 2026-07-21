using Lumbre.Game.Client.Missions;
using Lumbre.Game.Domain.Missions;
using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    public sealed class H7NaraPresentation : MonoBehaviour
    {
        [SerializeField] private H5NaraController nara;
        [SerializeField] private Transform player;
        [SerializeField] private Transform visual;
        [SerializeField] private SpriteRenderer missionIndicator;

        public bool IsConfigured => nara != null && player != null && visual != null
            && missionIndicator != null;

        public void Configure(H5NaraController missionNpc, Transform playerTransform,
            Transform visualTransform, SpriteRenderer indicator)
        {
            nara = missionNpc;
            player = playerTransform;
            visual = visualTransform;
            missionIndicator = indicator;
        }

        private void Awake()
        {
            nara ??= GetComponent<H5NaraController>();
            player ??= GameObject.FindWithTag("Player")?.transform;
            visual ??= transform.Find("H7_Nara_Visual");
            missionIndicator ??= GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (nara == null || player == null || visual == null || missionIndicator == null)
            {
                return;
            }

            var delta = player.position - transform.position;
            if (Mathf.Abs(delta.x) > 0.02f && delta.sqrMagnitude <= 16f)
            {
                var scale = visual.localScale;
                scale.x = Mathf.Abs(scale.x) * (delta.x < 0f ? -1f : 1f);
                visual.localScale = scale;
            }

            var state = nara.Runtime?.Mission?.State ?? MissionState.Completed;
            var visible = state == MissionState.Available || state == MissionState.ReadyToTurnIn;
            missionIndicator.enabled = visible;
            if (visible)
            {
                var pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.1f;
                missionIndicator.transform.localScale = Vector3.one * pulse;
                missionIndicator.color = state == MissionState.ReadyToTurnIn
                    ? new Color(1f, 0.75f, 0.2f, 1f)
                    : new Color(0.25f, 0.95f, 0.85f, 1f);
            }
        }
    }
}
