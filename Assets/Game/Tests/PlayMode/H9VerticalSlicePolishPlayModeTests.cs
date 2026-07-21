using System.Collections;
using Lumbre.Game.Client.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Lumbre.Game.Tests
{
    public sealed class H9VerticalSlicePolishPlayModeTests
    {
        [UnityTest]
        public IEnumerator PresentationPolishKeepsCameraHudControlsAndSafeAreaConfigured()
        {
            SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
            yield return null;
            yield return new WaitForFixedUpdate();

            var canvasObject = GameObject.Find("H3_MobileControls");
            var safeRoot = GameObject.Find("H9_SafeAreaRoot");
            var cameraObject = GameObject.Find("CM_OfficialCamera");
            var statusPanel = GameObject.Find("H7_StatusPanel");
            var missionPanel = GameObject.Find("H7_MissionPanel");
            var joystick = GameObject.Find("VirtualJoystick_Base");
            var attack = GameObject.Find("H4_AttackButton");
            var defense = GameObject.Find("H4B_DefenseButton");

            try
            {
                Assert.IsNotNull(canvasObject);
                var canvas = canvasObject.GetComponent<Canvas>();
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                Assert.IsNotNull(canvas);
                Assert.IsNotNull(scaler);
                Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920f, 1080f)));
                Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(0.45f).Within(0.001f));

                var safeArea = safeRoot?.GetComponent<H9SafeAreaLayout>();
                Assert.IsNotNull(safeArea);
                Assert.IsTrue(safeArea.IsConfigured);
                Assert.That(safeArea.AppliedSafeArea.width, Is.GreaterThan(0f));
                Assert.That(safeArea.AppliedSafeArea.height, Is.GreaterThan(0f));

                var polish = cameraObject?.GetComponent<H7CameraPolish>();
                var confiner = cameraObject?.GetComponent(
                    "Unity.Cinemachine.CinemachineConfiner2D");
                Assert.IsNotNull(polish);
                Assert.IsTrue(polish.IsPolished);
                Assert.That(polish.OrthographicSize, Is.EqualTo(8.2f).Within(0.05f));
                Assert.That(polish.FollowOffset.y, Is.GreaterThan(0.1f));
                Assert.IsNotNull(confiner);

                Assert.IsNotNull(statusPanel);
                Assert.IsNotNull(missionPanel);
                Assert.That(statusPanel.GetComponent<RectTransform>().sizeDelta.x,
                    Is.LessThanOrEqualTo(660f));
                Assert.That(missionPanel.GetComponent<RectTransform>().sizeDelta.y,
                    Is.LessThanOrEqualTo(100f));
                Assert.IsTrue(statusPanel.GetComponent<H7StatusHud>().IsConfigured);

                Assert.IsNotNull(joystick);
                Assert.That(joystick.GetComponent<RectTransform>().sizeDelta.x,
                    Is.GreaterThanOrEqualTo(220f));
                Assert.IsNotNull(attack);
                Assert.IsNotNull(defense);
                Assert.That(attack.GetComponent<RectTransform>().sizeDelta.x,
                    Is.GreaterThanOrEqualTo(140f));
                Assert.That(defense.GetComponent<RectTransform>().sizeDelta.x,
                    Is.GreaterThanOrEqualTo(140f));
                Assert.That(Mathf.Abs(attack.GetComponent<RectTransform>().anchoredPosition.x
                    - defense.GetComponent<RectTransform>().anchoredPosition.x), Is.GreaterThan(120f));
                Assert.That(H8PauseController.VersionLabel, Is.EqualTo("v0.8.1 Alpha"));
            }
            finally
            {
                Time.timeScale = 1f;
            }
        }
    }
}
