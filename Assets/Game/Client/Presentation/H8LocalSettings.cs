using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    public struct H8SettingsSnapshot
    {
        public H8SettingsSnapshot(float musicVolume, float fxVolume, bool vibrationEnabled,
            bool showFps, bool showDebug, int qualityLevel)
        {
            MusicVolume = musicVolume;
            FxVolume = fxVolume;
            VibrationEnabled = vibrationEnabled;
            ShowFps = showFps;
            ShowDebug = showDebug;
            QualityLevel = qualityLevel;
        }

        public float MusicVolume { get; }
        public float FxVolume { get; }
        public bool VibrationEnabled { get; }
        public bool ShowFps { get; }
        public bool ShowDebug { get; }
        public int QualityLevel { get; }
    }

    public static class H8LocalSettings
    {
        public const string MusicVolumeKey = "lumbre.h8.musicVolume";
        public const string FxVolumeKey = "lumbre.h8.fxVolume";
        public const string VibrationKey = "lumbre.h8.vibration";
        public const string ShowFpsKey = "lumbre.h8.showFps";
        public const string ShowDebugKey = "lumbre.h8.showDebug";
        public const string QualityKey = "lumbre.h8.quality";

        public static H8SettingsSnapshot Defaults => new H8SettingsSnapshot(
            1f, 1f, true, false, false, DefaultQualityLevel);

        public static H8SettingsSnapshot Load()
        {
            var defaults = Defaults;
            return new H8SettingsSnapshot(
                Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, defaults.MusicVolume)),
                Mathf.Clamp01(PlayerPrefs.GetFloat(FxVolumeKey, defaults.FxVolume)),
                PlayerPrefs.GetInt(VibrationKey, defaults.VibrationEnabled ? 1 : 0) != 0,
                PlayerPrefs.GetInt(ShowFpsKey, defaults.ShowFps ? 1 : 0) != 0,
                PlayerPrefs.GetInt(ShowDebugKey, defaults.ShowDebug ? 1 : 0) != 0,
                Mathf.Max(0, PlayerPrefs.GetInt(QualityKey, defaults.QualityLevel)));
        }

        public static void Save(H8SettingsSnapshot settings)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(settings.MusicVolume));
            PlayerPrefs.SetFloat(FxVolumeKey, Mathf.Clamp01(settings.FxVolume));
            PlayerPrefs.SetInt(VibrationKey, settings.VibrationEnabled ? 1 : 0);
            PlayerPrefs.SetInt(ShowFpsKey, settings.ShowFps ? 1 : 0);
            PlayerPrefs.SetInt(ShowDebugKey, settings.ShowDebug ? 1 : 0);
            PlayerPrefs.SetInt(QualityKey, Mathf.Max(0, settings.QualityLevel));
            PlayerPrefs.Save();
        }

        public static void ResetForQa()
        {
            PlayerPrefs.DeleteKey(MusicVolumeKey);
            PlayerPrefs.DeleteKey(FxVolumeKey);
            PlayerPrefs.DeleteKey(VibrationKey);
            PlayerPrefs.DeleteKey(ShowFpsKey);
            PlayerPrefs.DeleteKey(ShowDebugKey);
            PlayerPrefs.DeleteKey(QualityKey);
            PlayerPrefs.Save();
        }

        private static int DefaultQualityLevel => QualitySettings.names.Length == 0
            ? 0
            : Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, QualitySettings.names.Length - 1);
    }
}
