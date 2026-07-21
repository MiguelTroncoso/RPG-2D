using System.Collections;
using Lumbre.Game.Client.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H8UxSettingsPlayModeTests
    {
        [UnityTest]
        public IEnumerator PauseOptionsAndSettingsRemainFunctional()
        {
            H8LocalSettings.ResetForQa();
            SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
            yield return null;
            yield return new WaitForFixedUpdate();

            var controller = GameObject.Find("H8_UXRoot")
                .GetComponent<H8PauseController>();
            var overlay = GameObject.Find("H8_UXRoot")
                .GetComponent<H8PerformanceOverlay>();
            try
            {
                Assert.IsTrue(controller.IsConfigured);
                Assert.IsTrue(overlay.IsConfigured);
                Assert.IsTrue(GameObject.Find("H7_StatusPanel")
                    .GetComponent<H7StatusHud>().IsConfigured);
                Assert.IsTrue(GameObject.Find("H8_MusicSlider")
                    .GetComponent<H8Tooltip>().IsConfigured);
                Assert.IsTrue(GameObject.Find("H8_GraphicsButton")
                    .GetComponent<H8Tooltip>().IsConfigured);

                controller.SetMusicVolume(0.35f);
                controller.SetFxVolume(0.65f);
                controller.SetVibration(false);
                controller.SetShowFps(true);
                controller.SetShowDebug(true);

                var settings = H8LocalSettings.Load();
                Assert.That(settings.MusicVolume, Is.EqualTo(0.35f).Within(0.001f));
                Assert.That(settings.FxVolume, Is.EqualTo(0.65f).Within(0.001f));
                Assert.IsFalse(settings.VibrationEnabled);
                Assert.IsTrue(settings.ShowFps);
                Assert.IsTrue(settings.ShowDebug);
                Assert.IsTrue(overlay.IsVisible);

                yield return new WaitForSecondsRealtime(0.6f);
                StringAssert.Contains("CPU", overlay.LastReport);
                StringAssert.Contains("GPU", overlay.LastReport);

                controller.TogglePause();
                Assert.IsTrue(controller.IsPaused);
                Assert.That(Time.timeScale, Is.EqualTo(0f));

                controller.OpenOptions();
                Assert.IsTrue(controller.OptionsVisible);
                controller.CloseOptions();
                Assert.IsFalse(controller.OptionsVisible);
                Assert.IsTrue(controller.IsPaused);

                controller.Resume();
                Assert.IsFalse(controller.IsPaused);
                Assert.That(Time.timeScale, Is.EqualTo(1f));

                controller.ExitGame();
                Assert.IsTrue(controller.ExitRequested);
            }
            finally
            {
                controller?.Resume();
                H8LocalSettings.ResetForQa();
                Time.timeScale = 1f;
            }
        }
    }
}
