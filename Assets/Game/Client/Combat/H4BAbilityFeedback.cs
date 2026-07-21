using System.Collections;
using Lumbre.Game.Domain.Combat;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class H4BAbilityFeedback : MonoBehaviour
    {
        [SerializeField] private Color defenseColor = new Color(0.25f, 0.85f, 1f, 1f);
        [SerializeField] private Color areaColor = new Color(1f, 0.28f, 0.85f, 1f);
        [SerializeField, Min(0.05f)] private float ringDuration = 0.25f;

        private AudioSource _audioSource;
        private AudioClip _abilityClip;
        private LineRenderer _ring;
        private Coroutine _ringRoutine;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _ring = GetComponent<LineRenderer>();
            if (_ring == null)
            {
                _ring = gameObject.AddComponent<LineRenderer>();
            }

            _ring.useWorldSpace = true;
            _ring.loop = true;
            _ring.positionCount = 32;
            _ring.startWidth = 0.06f;
            _ring.endWidth = 0.06f;
            _ring.enabled = false;
            _ring.sortingOrder = 30;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _ring.material = new Material(shader);
            }
        }

        public void PlayDefense(float duration)
        {
            PlayTone(420f, 0.14f, 0.22f);
            StartCoroutine(DefensePulse(duration));
        }

        public void PlayArea(float radius)
        {
            PlayTone(760f, 0.18f, 0.24f);
            if (_ringRoutine != null)
            {
                StopCoroutine(_ringRoutine);
            }

            _ringRoutine = StartCoroutine(AreaRing(radius));
        }

        public void PlayHeatGain(int amount)
        {
            if (amount > 0)
            {
                PlayTone(520f, 0.04f, 0.08f);
            }
        }

        public void PlayRejected(AbilityResultCode code)
        {
            PlayTone(120f, 0.05f, 0.1f);
        }

        public void PlayWaveWarning()
        {
            PlayTone(180f, 0.22f, 0.18f);
        }

        public void PlayWaveImpact()
        {
            PlayTone(90f, 0.3f, 0.28f);
        }

        private IEnumerator DefensePulse(float duration)
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            var original = new Color[renderers.Length];
            for (var index = 0; index < renderers.Length; index++)
            {
                original[index] = renderers[index].material.color;
                renderers[index].material.color = defenseColor;
            }

            yield return new WaitForSeconds(Mathf.Min(0.2f, Mathf.Max(0.05f, duration)));
            for (var index = 0; index < renderers.Length; index++)
            {
                renderers[index].material.color = original[index];
            }
        }

        private IEnumerator AreaRing(float radius)
        {
            _ring.enabled = true;
            _ring.startColor = areaColor;
            _ring.endColor = areaColor;
            var elapsed = 0f;
            while (elapsed < ringDuration)
            {
                var progress = elapsed / ringDuration;
                SetRing(radius * Mathf.Lerp(0.35f, 1f, progress));
                elapsed += Time.deltaTime;
                yield return null;
            }

            _ring.enabled = false;
            _ringRoutine = null;
        }

        private void SetRing(float radius)
        {
            var center = transform.position + Vector3.forward * 0.1f;
            for (var index = 0; index < _ring.positionCount; index++)
            {
                var angle = index / (float)_ring.positionCount * Mathf.PI * 2f;
                _ring.SetPosition(index, center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f));
            }
        }

        private void PlayTone(float frequency, float duration, float volume)
        {
            if (_audioSource == null)
            {
                return;
            }

            _abilityClip = CreateTone(frequency, duration);
            _audioSource.PlayOneShot(_abilityClip, volume);
        }

        private static AudioClip CreateTone(float frequency, float duration)
        {
            const int sampleRate = 22050;
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
            var samples = new float[sampleCount];
            for (var index = 0; index < samples.Length; index++)
            {
                var envelope = 1f - index / (float)samples.Length;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * index / sampleRate)
                    * envelope;
            }

            var clip = AudioClip.Create("H4B_AbilityTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
