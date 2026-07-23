using System.Collections.Generic;
using Lumbre.Game.Client.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Presentation
{
    public sealed class H7AudioFeedback : MonoBehaviour
    {
        [SerializeField] private H3PlayerController player;
        [SerializeField] private float ambientFadeSpeed = 1.8f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float fxVolume = 1f;

        private readonly List<Button> _boundButtons = new List<Button>();
        private AudioSource _oneShotSource;
        private AudioSource[] _ambientSources;
        private AudioClip _walkClip;
        private AudioClip _attackClip;
        private AudioClip _impactClip;
        private AudioClip _deathClip;
        private AudioClip _missionClip;
        private AudioClip _levelClip;
        private AudioClip _equipClip;
        private AudioClip _uiClip;
        private AudioClip[] _ambientClips;
        private Vector3[] _zoneCenters;
        private int _activeZone;
        private float _stepTimer;

        public bool IsConfigured => _oneShotSource != null
            && _ambientSources != null
            && _ambientSources.Length == 3
            && _ambientClips != null;
        public int AmbientSourceCount => _ambientSources?.Length ?? 0;
        public float MusicVolume => musicVolume;
        public float FxVolume => fxVolume;

        public void Configure(H3PlayerController playerController,
            Vector3 plazaCenter, Vector3 trailCenter, Vector3 caveCenter)
        {
            player = playerController;
            _zoneCenters = new[] { plazaCenter, trailCenter, caveCenter };
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            if (_ambientSources == null)
            {
                return;
            }

            for (var index = 0; index < _ambientSources.Length; index++)
            {
                var target = index == _activeZone ? 0.12f * musicVolume : 0f;
                _ambientSources[index].volume = Mathf.Min(_ambientSources[index].volume, target);
            }
        }

        public void SetFxVolume(float value)
        {
            fxVolume = Mathf.Clamp01(value);
            if (_oneShotSource != null)
            {
                _oneShotSource.volume = 0.7f * fxVolume;
            }
        }

        private void Awake()
        {
            player ??= FindFirstObjectByType<H3PlayerController>();
            EnsureAudio();
        }

        private void Start()
        {
            BindUiButtons();
            if (_zoneCenters == null || _zoneCenters.Length != 3)
            {
                _zoneCenters = new[] { transform.position, transform.position + Vector3.right * 8f,
                    transform.position + Vector3.right * 16f };
            }

            for (var index = 0; index < _ambientSources.Length; index++)
            {
                _ambientSources[index].clip = _ambientClips[index];
                _ambientSources[index].loop = true;
                _ambientSources[index].volume = index == _activeZone ? 0.12f * musicVolume : 0f;
                _ambientSources[index].Play();
            }
        }

        private void Update()
        {
            if (player != null && player.CurrentWorldDirection.sqrMagnitude > 0.01f)
            {
                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    PlayWalk();
                    _stepTimer = 0.28f;
                }
            }
            else
            {
                _stepTimer = 0f;
            }

            UpdateAmbientZone();
        }

        public void PlayWalk()
        {
            Play(_walkClip, 0.1f);
        }

        public void PlayAttack()
        {
            Play(_attackClip, 0.22f);
        }

        public void PlayImpact()
        {
            Play(_impactClip, 0.24f);
        }

        public void PlayDeath()
        {
            Play(_deathClip, 0.28f);
        }

        public void PlayMission()
        {
            Play(_missionClip, 0.28f);
        }

        public void PlayLevelUp()
        {
            Play(_levelClip, 0.34f);
        }

        public void PlayEquip()
        {
            Play(_equipClip, 0.24f);
        }

        public void PlayUi()
        {
            Play(_uiClip, 0.1f);
        }

        public void PlayRejected()
        {
            Play(_uiClip, 0.06f);
        }

        private void EnsureAudio()
        {
            _oneShotSource = GetComponent<AudioSource>();
            if (_oneShotSource == null)
            {
                _oneShotSource = gameObject.AddComponent<AudioSource>();
            }

            _oneShotSource.playOnAwake = false;
            _oneShotSource.spatialBlend = 0f;
            _oneShotSource.volume = 0.7f * fxVolume;

            _ambientSources = new AudioSource[3];
            for (var index = 0; index < _ambientSources.Length; index++)
            {
                var sourceObject = new GameObject("H7_Ambient_" + index);
                sourceObject.transform.SetParent(transform, false);
                var source = sourceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                _ambientSources[index] = source;
            }

            _walkClip = CreateTone("H7_Walk", 170f, 0.045f, 0.2f);
            _attackClip = CreateTone("H7_Attack", 420f, 0.12f, 0.35f);
            _impactClip = CreateTone("H7_Impact", 95f, 0.16f, 0.38f);
            _deathClip = CreateTone("H7_Death", 65f, 0.34f, 0.35f);
            _missionClip = CreateTone("H7_Mission", 560f, 0.24f, 0.32f);
            _levelClip = CreateTone("H7_LevelUp", 740f, 0.42f, 0.38f);
            _equipClip = CreateTone("H7_Equip", 300f, 0.18f, 0.3f);
            _uiClip = CreateTone("H7_UI", 260f, 0.045f, 0.16f);
            _ambientClips = new[]
            {
                CreateAmbient("H7_Ambient_Plaza", 120f, 0.1f),
                CreateAmbient("H7_Ambient_Trail", 95f, 0.08f),
                CreateAmbient("H7_Ambient_Cave", 70f, 0.12f)
            };
        }

        private void BindUiButtons()
        {
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            for (var index = 0; index < buttons.Length; index++)
            {
                if (_boundButtons.Contains(buttons[index]))
                {
                    continue;
                }

                buttons[index].onClick.AddListener(PlayUi);
                _boundButtons.Add(buttons[index]);
            }
        }

        private void UpdateAmbientZone()
        {
            if (player == null || _zoneCenters == null || _zoneCenters.Length != 3)
            {
                return;
            }

            var nearest = 0;
            var nearestDistance = float.PositiveInfinity;
            for (var index = 0; index < _zoneCenters.Length; index++)
            {
                var distance = (player.transform.position - _zoneCenters[index]).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = index;
                    nearestDistance = distance;
                }
            }

            _activeZone = nearest;
            for (var index = 0; index < _ambientSources.Length; index++)
            {
                var target = index == _activeZone ? 0.12f * musicVolume : 0f;
                _ambientSources[index].volume = Mathf.MoveTowards(
                    _ambientSources[index].volume, target, ambientFadeSpeed * Time.deltaTime);
            }
        }

        private void Play(AudioClip clip, float volume)
        {
            if (_oneShotSource != null && clip != null)
            {
                _oneShotSource.PlayOneShot(clip, volume);
            }
        }

        private static AudioClip CreateTone(string name, float frequency, float duration, float amplitude)
        {
            const int sampleRate = 22050;
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
            var samples = new float[sampleCount];
            for (var index = 0; index < samples.Length; index++)
            {
                var progress = index / (float)samples.Length;
                var envelope = 1f - progress;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * index / sampleRate)
                    * envelope * amplitude;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateAmbient(string name, float frequency, float amplitude)
        {
            const int sampleRate = 11025;
            const int sampleCount = sampleRate * 2;
            var samples = new float[sampleCount];
            for (var index = 0; index < samples.Length; index++)
            {
                var time = index / (float)sampleRate;
                var swell = 0.5f + Mathf.Sin(time * Mathf.PI) * 0.5f;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * time)
                    * swell * amplitude;
            }

            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            for (var index = 0; index < _boundButtons.Count; index++)
            {
                if (_boundButtons[index] != null)
                {
                    _boundButtons[index].onClick.RemoveListener(PlayUi);
                }
            }
        }
    }
}
