using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Presentation
{
    public sealed class H8PauseController : MonoBehaviour
    {
        public const string VersionLabel = "v0.8.0 Alpha";

        [SerializeField] private CanvasGroup pausePanel;
        [SerializeField] private CanvasGroup optionsPanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button graphicsButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider fxSlider;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Toggle fpsToggle;
        [SerializeField] private Toggle debugToggle;
        [SerializeField] private Text graphicsLabel;
        [SerializeField] private Text versionLabel;
        [SerializeField] private H7AudioFeedback audioFeedback;
        [SerializeField] private H8PerformanceOverlay performanceOverlay;

        private H8SettingsSnapshot _settings;
        private bool _bound;

        public bool IsConfigured => pausePanel != null && optionsPanel != null
            && pauseButton != null && continueButton != null && optionsButton != null
            && backButton != null && exitButton != null && graphicsButton != null
            && musicSlider != null && fxSlider != null && vibrationToggle != null
            && fpsToggle != null && debugToggle != null && graphicsLabel != null
            && versionLabel != null && audioFeedback != null && performanceOverlay != null;
        public bool IsPaused { get; private set; }
        public bool OptionsVisible { get; private set; }
        public bool ExitRequested { get; private set; }
        public H8SettingsSnapshot CurrentSettings => _settings;

        public void Configure(CanvasGroup pause, CanvasGroup options, Button pauseControl,
            Button continueControl, Button optionsControl, Button backControl, Button exitControl,
            Button qualityControl, Slider music, Slider fx, Toggle vibration, Toggle fps,
            Toggle debug, Text qualityText, Text versionText, H7AudioFeedback audio,
            H8PerformanceOverlay overlay)
        {
            pausePanel = pause;
            optionsPanel = options;
            pauseButton = pauseControl;
            continueButton = continueControl;
            optionsButton = optionsControl;
            backButton = backControl;
            exitButton = exitControl;
            graphicsButton = qualityControl;
            musicSlider = music;
            fxSlider = fx;
            vibrationToggle = vibration;
            fpsToggle = fps;
            debugToggle = debug;
            graphicsLabel = qualityText;
            versionLabel = versionText;
            audioFeedback = audio;
            performanceOverlay = overlay;
        }

        private void Awake()
        {
            audioFeedback ??= FindFirstObjectByType<H7AudioFeedback>();
            performanceOverlay ??= FindFirstObjectByType<H8PerformanceOverlay>();
            _settings = H8LocalSettings.Load();
            ApplySettings();
            SetPaused(false);
        }

        private void Start()
        {
            BindControls();
            ApplySettingsToControls();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            SetPaused(!IsPaused);
        }

        public void Resume()
        {
            SetPaused(false);
        }

        public void OpenOptions()
        {
            if (!IsPaused)
            {
                SetPaused(true);
            }

            OptionsVisible = true;
            SetPanel(pausePanel, false);
            SetPanel(optionsPanel, true);
        }

        public void CloseOptions()
        {
            OptionsVisible = false;
            SetPanel(optionsPanel, false);
            SetPanel(pausePanel, IsPaused);
        }

        public void SetMusicVolume(float value)
        {
            _settings = new H8SettingsSnapshot(value, _settings.FxVolume,
                _settings.VibrationEnabled, _settings.ShowFps, _settings.ShowDebug,
                _settings.QualityLevel);
            H8LocalSettings.Save(_settings);
            audioFeedback?.SetMusicVolume(_settings.MusicVolume);
        }

        public void SetFxVolume(float value)
        {
            _settings = new H8SettingsSnapshot(_settings.MusicVolume, value,
                _settings.VibrationEnabled, _settings.ShowFps, _settings.ShowDebug,
                _settings.QualityLevel);
            H8LocalSettings.Save(_settings);
            audioFeedback?.SetFxVolume(_settings.FxVolume);
        }

        public void SetVibration(bool enabled)
        {
            _settings = new H8SettingsSnapshot(_settings.MusicVolume, _settings.FxVolume,
                enabled, _settings.ShowFps, _settings.ShowDebug, _settings.QualityLevel);
            H8LocalSettings.Save(_settings);
            ApplySettingsToControls();
        }

        public void SetShowFps(bool enabled)
        {
            _settings = new H8SettingsSnapshot(_settings.MusicVolume, _settings.FxVolume,
                _settings.VibrationEnabled, enabled, _settings.ShowDebug, _settings.QualityLevel);
            H8LocalSettings.Save(_settings);
            performanceOverlay?.SetVisibility(_settings.ShowFps, _settings.ShowDebug);
            ApplySettingsToControls();
        }

        public void SetShowDebug(bool enabled)
        {
            _settings = new H8SettingsSnapshot(_settings.MusicVolume, _settings.FxVolume,
                _settings.VibrationEnabled, _settings.ShowFps, enabled, _settings.QualityLevel);
            H8LocalSettings.Save(_settings);
            performanceOverlay?.SetVisibility(_settings.ShowFps, _settings.ShowDebug);
            ApplySettingsToControls();
        }

        public void CycleGraphicsQuality()
        {
            var qualityCount = QualitySettings.names.Length;
            var nextQuality = qualityCount == 0
                ? 0
                : (_settings.QualityLevel + 1) % qualityCount;
            _settings = new H8SettingsSnapshot(_settings.MusicVolume, _settings.FxVolume,
                _settings.VibrationEnabled, _settings.ShowFps, _settings.ShowDebug, nextQuality);
            H8LocalSettings.Save(_settings);
            ApplySettings();
            ApplySettingsToControls();
        }

        public void ExitGame()
        {
            ExitRequested = true;
            if (!UnityEngine.Application.isEditor)
            {
                UnityEngine.Application.Quit();
            }
        }

        private void BindControls()
        {
            if (_bound)
            {
                return;
            }

            _bound = true;
            pauseButton.onClick.AddListener(TogglePause);
            continueButton.onClick.AddListener(Resume);
            optionsButton.onClick.AddListener(OpenOptions);
            backButton.onClick.AddListener(CloseOptions);
            exitButton.onClick.AddListener(ExitGame);
            graphicsButton.onClick.AddListener(CycleGraphicsQuality);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            fxSlider.onValueChanged.AddListener(SetFxVolume);
            vibrationToggle.onValueChanged.AddListener(SetVibration);
            fpsToggle.onValueChanged.AddListener(SetShowFps);
            debugToggle.onValueChanged.AddListener(SetShowDebug);
        }

        private void ApplySettings()
        {
            audioFeedback?.SetMusicVolume(_settings.MusicVolume);
            audioFeedback?.SetFxVolume(_settings.FxVolume);
            performanceOverlay?.SetVisibility(_settings.ShowFps, _settings.ShowDebug);
            if (QualitySettings.names.Length > 0)
            {
                QualitySettings.SetQualityLevel(Mathf.Clamp(_settings.QualityLevel, 0,
                    QualitySettings.names.Length - 1), true);
            }

            UnityEngine.Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        private void ApplySettingsToControls()
        {
            if (!IsConfigured)
            {
                return;
            }

            musicSlider.SetValueWithoutNotify(_settings.MusicVolume);
            fxSlider.SetValueWithoutNotify(_settings.FxVolume);
            vibrationToggle.SetIsOnWithoutNotify(_settings.VibrationEnabled);
            fpsToggle.SetIsOnWithoutNotify(_settings.ShowFps);
            debugToggle.SetIsOnWithoutNotify(_settings.ShowDebug);
            var qualityName = QualitySettings.names.Length == 0
                ? "AUTO"
                : QualitySettings.names[Mathf.Clamp(_settings.QualityLevel, 0,
                    QualitySettings.names.Length - 1)];
            graphicsLabel.text = "GRÁFICOS: " + qualityName.ToUpperInvariant();
            versionLabel.text = VersionLabel;
        }

        private void SetPaused(bool paused)
        {
            IsPaused = paused;
            if (!paused)
            {
                OptionsVisible = false;
            }

            Time.timeScale = paused ? 0f : 1f;
            SetPanel(pausePanel, paused && !OptionsVisible);
            SetPanel(optionsPanel, paused && OptionsVisible);
        }

        private static void SetPanel(CanvasGroup panel, bool visible)
        {
            if (panel == null)
            {
                return;
            }

            panel.alpha = visible ? 1f : 0f;
            panel.interactable = visible;
            panel.blocksRaycasts = visible;
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (!_bound)
            {
                return;
            }

            pauseButton.onClick.RemoveListener(TogglePause);
            continueButton.onClick.RemoveListener(Resume);
            optionsButton.onClick.RemoveListener(OpenOptions);
            backButton.onClick.RemoveListener(CloseOptions);
            exitButton.onClick.RemoveListener(ExitGame);
            graphicsButton.onClick.RemoveListener(CycleGraphicsQuality);
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            fxSlider.onValueChanged.RemoveListener(SetFxVolume);
            vibrationToggle.onValueChanged.RemoveListener(SetVibration);
            fpsToggle.onValueChanged.RemoveListener(SetShowFps);
            debugToggle.onValueChanged.RemoveListener(SetShowDebug);
        }
    }
}
