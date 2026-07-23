using System.Collections;
using Lumbre.Game.Domain.Combat;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class H4CombatFeedback : MonoBehaviour
    {
        [SerializeField] private Color hitColor = new Color(1f, 0.3f, 0.22f, 1f);
        [SerializeField] private Color deathColor = new Color(0.35f, 0.08f, 0.08f, 1f);
        [SerializeField, Min(0.01f)] private float hitDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float deathDuration = 0.28f;

        private H4CombatHealth _health;
        private Renderer[] _renderers;
        private Color[] _baseColors;
        private AudioSource _audioSource;
        private AudioClip _hitClip;
        private AudioClip _deathClip;
        private Transform _visualTransform;
        private Vector3 _visualBaseLocalPosition;
        private bool _dead;

        private void Awake()
        {
            _health = GetComponent<H4CombatHealth>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _baseColors = new Color[_renderers.Length];
            for (var index = 0; index < _renderers.Length; index++)
            {
                _baseColors[index] = _renderers[index].material.color;
            }

            for (var index = 0; index < _renderers.Length; index++)
            {
                if (_renderers[index] != null)
                {
                    _visualTransform = _renderers[index].transform;
                    break;
                }
            }

            _visualTransform ??= transform;
            _visualBaseLocalPosition = _visualTransform.localPosition;

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            if (_health == null)
            {
                return;
            }

            _health.DamageTaken += HandleDamage;
            _health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (_health == null)
            {
                return;
            }

            _health.DamageTaken -= HandleDamage;
            _health.Died -= HandleDeath;
        }

        public void PlayAttack()
        {
            if (_dead)
            {
                return;
            }

            StartCoroutine(AttackPulse());
            PlayTone(ref _hitClip, 620f, 0.06f, 0.18f);
        }

        public void PlayAttackAnticipation()
        {
            if (_dead || _visualTransform == null)
            {
                return;
            }

            StartCoroutine(AttackAnticipationPulse());
        }

        private void HandleDamage(DamageResult result)
        {
            if (result.Killed)
            {
                return;
            }

            StartCoroutine(Flash(hitColor, hitDuration));
            PlayTone(ref _hitClip, 220f, 0.08f, 0.22f);
        }

        private void HandleDeath()
        {
            if (_dead)
            {
                return;
            }

            _dead = true;
            StopAllCoroutines();
            StartCoroutine(DeathSequence());
            PlayTone(ref _deathClip, 110f, deathDuration, 0.26f);
        }

        private IEnumerator Flash(Color color, float duration)
        {
            SetRendererColor(color);
            yield return new WaitForSeconds(duration);
            RestoreRendererColors();
        }

        private IEnumerator AttackAnticipationPulse()
        {
            _visualTransform.localPosition = _visualBaseLocalPosition + Vector3.down * 0.02f;
            yield return new WaitForSeconds(0.06f);
            if (!_dead)
            {
                _visualTransform.localPosition = _visualBaseLocalPosition;
            }
        }

        private IEnumerator AttackPulse()
        {
            if (_visualTransform == null)
            {
                yield break;
            }

            _visualTransform.localPosition = _visualBaseLocalPosition + Vector3.up * 0.035f;
            yield return new WaitForSeconds(0.08f);
            if (!_dead)
            {
                _visualTransform.localPosition = _visualBaseLocalPosition;
            }
        }

        private IEnumerator DeathSequence()
        {
            SetRendererColor(deathColor);
            if (_visualTransform != null)
            {
                _visualTransform.localPosition = _visualBaseLocalPosition + Vector3.down * 0.04f;
                _visualTransform.localScale *= 0.8f;
            }
            yield return new WaitForSeconds(deathDuration);
            for (var index = 0; index < _renderers.Length; index++)
            {
                _renderers[index].enabled = false;
            }

            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private void SetRendererColor(Color color)
        {
            for (var index = 0; index < _renderers.Length; index++)
            {
                _renderers[index].material.color = color;
            }
        }

        private void RestoreRendererColors()
        {
            for (var index = 0; index < _renderers.Length; index++)
            {
                _renderers[index].material.color = _baseColors[index];
            }
        }

        private void PlayTone(ref AudioClip clip, float frequency, float duration, float volume)
        {
            if (_audioSource == null)
            {
                return;
            }

            clip ??= CreateTone(frequency, duration);
            _audioSource.PlayOneShot(clip, volume);
        }

        private static AudioClip CreateTone(float frequency, float duration)
        {
            const int sampleRate = 22050;
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
            var samples = new float[sampleCount];
            for (var index = 0; index < samples.Length; index++)
            {
                var envelope = 1f - (index / (float)samples.Length);
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * index / sampleRate)
                    * envelope;
            }

            var clip = AudioClip.Create("H4_CombatTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
