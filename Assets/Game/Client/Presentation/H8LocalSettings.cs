using System;
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
            try
            {
                var defaults = Defaults;
                return new H8SettingsSnapshot(
                    ReadFiniteNormalized(MusicVolumeKey, defaults.MusicVolume),
                    ReadFiniteNormalized(FxVolumeKey, defaults.FxVolume),
                    ReadToggle(VibrationKey, defaults.VibrationEnabled),
                    ReadToggle(ShowFpsKey, defaults.ShowFps),
                    ReadToggle(ShowDebugKey, defaults.ShowDebug),
                    ReadQuality(defaults.QualityLevel));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[H8] Local settings fallback applied: {exception.Message}");
                return Defaults;
            }
        }

        public static void Save(H8SettingsSnapshot settings)
        {
            try
            {
                PlayerPrefs.SetFloat(MusicVolumeKey, ClampFinite(settings.MusicVolume, 1f));
                PlayerPrefs.SetFloat(FxVolumeKey, ClampFinite(settings.FxVolume, 1f));
                PlayerPrefs.SetInt(VibrationKey, settings.VibrationEnabled ? 1 : 0);
                PlayerPrefs.SetInt(ShowFpsKey, settings.ShowFps ? 1 : 0);
                PlayerPrefs.SetInt(ShowDebugKey, settings.ShowDebug ? 1 : 0);
                PlayerPrefs.SetInt(QualityKey, SanitizeQuality(settings.QualityLevel));
                PlayerPrefs.Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[H8] Local settings save skipped: {exception.Message}");
            }
        }

        public static void ResetForQa()
        {
            try
            {
                PlayerPrefs.DeleteKey(MusicVolumeKey);
                PlayerPrefs.DeleteKey(FxVolumeKey);
                PlayerPrefs.DeleteKey(VibrationKey);
                PlayerPrefs.DeleteKey(ShowFpsKey);
                PlayerPrefs.DeleteKey(ShowDebugKey);
                PlayerPrefs.DeleteKey(QualityKey);
                PlayerPrefs.Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[H8] Local settings reset skipped: {exception.Message}");
            }
        }

        private static int DefaultQualityLevel => QualitySettings.names.Length == 0
            ? 0
            : Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, QualitySettings.names.Length - 1);

        private static float ReadFiniteNormalized(string key, float fallback)
        {
            return ClampFinite(PlayerPrefs.GetFloat(key, fallback), fallback);
        }

        private static bool ReadToggle(string key, bool fallback)
        {
            return PlayerPrefs.GetInt(key, fallback ? 1 : 0) != 0;
        }

        private static int ReadQuality(int fallback)
        {
            return SanitizeQuality(PlayerPrefs.GetInt(QualityKey, fallback));
        }

        private static float ClampFinite(float value, float fallback)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp01(value);
        }

        private static int SanitizeQuality(int value)
        {
            if (QualitySettings.names.Length == 0)
            {
                return 0;
            }

            return Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
        }
    }
}
