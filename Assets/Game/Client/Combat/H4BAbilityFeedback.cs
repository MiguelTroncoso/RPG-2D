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
        private Coroutine _defenseRoutine;
        private H4CombatHealth _health;
        private Renderer[] _defenseRenderers;
        private Color[] _defenseBaseColors;

        public bool IsDefenseVisualActive { get; private set; }
        public bool IsAreaPreviewActive { get; private set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _health = GetComponent<H4CombatHealth>();
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

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.Died += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Died -= HandleDeath;
            }

            StopDefenseVisual();
            StopAreaVisual();
        }

        public void PlayDefense(float duration)
        {
            PlayTone(420f, 0.14f, 0.22f);
            StopDefenseVisual();
            _defenseRoutine = StartCoroutine(DefensePulse(duration));
        }

        public void PlayAreaPreview(float radius, float duration)
        {
            PlayTone(300f, 0.1f, 0.14f);
            StopAreaVisual();
            _ringRoutine = StartCoroutine(AreaPreview(radius, duration));
        }

        public void PlayArea(float radius)
        {
            PlayTone(760f, 0.18f, 0.24f);
            StopAreaVisual();
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
            IsDefenseVisualActive = true;
            _defenseRenderers = GetComponentsInChildren<Renderer>(true);
            _defenseBaseColors = new Color[_defenseRenderers.Length];
            for (var index = 0; index < _defenseRenderers.Length; index++)
            {
                if (_defenseRenderers[index] == _ring)
                {
                    continue;
                }

                _defenseBaseColors[index] = _defenseRenderers[index].material.color;
                _defenseRenderers[index].material.color = defenseColor;
            }

            yield return new WaitForSeconds(Mathf.Max(0.05f, duration));
            RestoreDefenseRenderers();
            IsDefenseVisualActive = false;
            _defenseRoutine = null;
        }

        private IEnumerator AreaPreview(float radius, float duration)
        {
            IsAreaPreviewActive = true;
            _ring.enabled = true;
            var previewColor = areaColor;
            previewColor.a = 0.45f;
            _ring.startColor = previewColor;
            _ring.endColor = previewColor;
            var elapsed = 0f;
            var safeDuration = Mathf.Max(0.01f, duration);
            while (elapsed < safeDuration)
            {
                var pulse = 0.92f + Mathf.Sin(elapsed * 18f) * 0.08f;
                SetRing(radius * pulse);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _ring.enabled = false;
            IsAreaPreviewActive = false;
            _ringRoutine = null;
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
            IsAreaPreviewActive = false;
            _ringRoutine = null;
        }

        private void StopDefenseVisual()
        {
            if (_defenseRoutine != null)
            {
                StopCoroutine(_defenseRoutine);
                _defenseRoutine = null;
            }

            IsDefenseVisualActive = false;
            RestoreDefenseRenderers();
        }

        private void StopAreaVisual()
        {
            if (_ringRoutine != null)
            {
                StopCoroutine(_ringRoutine);
                _ringRoutine = null;
            }

            if (_ring != null)
            {
                _ring.enabled = false;
            }

            IsAreaPreviewActive = false;
        }

        private void HandleDeath()
        {
            StopDefenseVisual();
            StopAreaVisual();
        }

        private void RestoreDefenseRenderers()
        {
            if (_defenseRenderers == null || _defenseBaseColors == null)
            {
                return;
            }

            for (var index = 0; index < _defenseRenderers.Length; index++)
            {
                if (_defenseRenderers[index] != null && _defenseRenderers[index] != _ring)
                {
                    _defenseRenderers[index].material.color = _defenseBaseColors[index];
                }
            }
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
