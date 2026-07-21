using System.Collections;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Domain.Constants;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H8_1BootstrapPlayModeTests
    {
        [UnityTest]
        public IEnumerator BootstrapActivatesVerticalSliceAndInitializesPresentation()
        {
            H8LocalSettings.ResetForQa();
            PlayerPrefs.SetFloat(H8LocalSettings.MusicVolumeKey, float.NaN);
            PlayerPrefs.SetFloat(H8LocalSettings.FxVolumeKey, float.PositiveInfinity);
            PlayerPrefs.SetInt(H8LocalSettings.QualityKey, int.MaxValue);
            PlayerPrefs.Save();
            Time.timeScale = 0f;

            SceneManager.LoadScene(ProjectConstants.BootstrapSceneName, LoadSceneMode.Single);
            yield return new WaitForSecondsRealtime(0.5f);
            var waits = 0;
            while (SceneManager.GetActiveScene().name != ProjectConstants.VerticalSliceSceneName
                && waits++ < 20)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            yield return null;

            try
            {
                Assert.That(SceneUtility.GetBuildIndexByScenePath(ProjectConstants.BootstrapScenePath), Is.EqualTo(0));
                Assert.That(SceneUtility.GetBuildIndexByScenePath(ProjectConstants.VerticalSliceScenePath), Is.GreaterThanOrEqualTo(0));
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(ProjectConstants.VerticalSliceSceneName));
                Assert.That(Time.timeScale, Is.EqualTo(1f));

                var player = GameObject.FindWithTag("Player");
                var camera = Camera.main;
                var canvas = Object.FindFirstObjectByType<Canvas>();
                var eventSystem = Object.FindFirstObjectByType<EventSystem>();
                var pause = GameObject.Find("H8_UXRoot")?.GetComponent<H8PauseController>();
                var hud = GameObject.Find("H7_StatusPanel")?.GetComponent<H7StatusHud>();
                var pausePanel = GameObject.Find("H8_PausePanel")?.GetComponent<CanvasGroup>();
                var optionsPanel = GameObject.Find("H8_OptionsPanel")?.GetComponent<CanvasGroup>();
                var inputModule = eventSystem?.GetComponent(
                    "UnityEngine.InputSystem.UI.InputSystemUIInputModule");

                Assert.IsNotNull(player);
                Assert.IsNotNull(camera);
                Assert.IsTrue(camera.enabled);
                Assert.IsNotNull(camera.GetComponent("Unity.Cinemachine.CinemachineBrain"));
                Assert.IsTrue(GameObject.Find("CM_OfficialCamera")?.activeInHierarchy == true);
                Assert.IsNotNull(canvas);
                Assert.IsTrue(canvas.gameObject.activeInHierarchy);
                Assert.IsNotNull(eventSystem);
                Assert.IsNotNull(inputModule);
                Assert.IsTrue(pause != null && pause.IsConfigured);
                Assert.IsTrue(!pause.IsPaused && !pause.OptionsVisible);
                Assert.IsTrue(hud != null && hud.IsConfigured);
                Assert.IsNotNull(pausePanel);
                Assert.IsNotNull(optionsPanel);
                Assert.That(pausePanel.alpha, Is.EqualTo(0f));
                Assert.That(optionsPanel.alpha, Is.EqualTo(0f));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length,
                    Is.EqualTo(1));

                var settings = H8LocalSettings.Load();
                Assert.That(settings.MusicVolume, Is.EqualTo(1f));
                Assert.That(settings.FxVolume, Is.EqualTo(1f));
                Assert.That(settings.QualityLevel, Is.InRange(0,
                    Mathf.Max(0, QualitySettings.names.Length - 1)));
            }
            finally
            {
                H8LocalSettings.ResetForQa();
                Time.timeScale = 1f;
            }
        }
    }
}
