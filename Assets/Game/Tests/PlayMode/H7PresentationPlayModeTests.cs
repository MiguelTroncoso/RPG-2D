using System.Collections;
using System.IO;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Client.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H7PresentationPlayModeTests
    {
        [UnityTest]
        public IEnumerator PresentationLayerIsConfiguredAndSurvivesPersistenceReload()
        {
            var savePath = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                H6ProgressionRuntime.DefaultSaveFileName);
            var hadExistingSave = File.Exists(savePath);
            var existingSave = hadExistingSave ? File.ReadAllBytes(savePath) : null;

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            try
            {
                SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
                yield return null;
                yield return new WaitForFixedUpdate();

                AssertPresentationLayer();

                var progression = GameObject.Find("H6_ProgressionRuntime")
                    .GetComponent<H6ProgressionRuntime>();
                progression.SaveNow();
                Assert.IsTrue(File.Exists(savePath));

                SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
                yield return null;
                yield return new WaitForFixedUpdate();
                yield return null;

                AssertPresentationLayer();
                Assert.AreEqual(
                    Lumbre.Game.Application.Persistence.SaveLoadStatus.Loaded,
                    GameObject.Find("H6_ProgressionRuntime")
                        .GetComponent<H6ProgressionRuntime>().LastLoadStatus);
            }
            finally
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                if (hadExistingSave)
                {
                    File.WriteAllBytes(savePath, existingSave);
                }
            }
        }

        private static void AssertPresentationLayer()
        {
            var presentation = GameObject.Find("H7_Presentation")
                .GetComponent<H7PresentationRuntime>();
            Assert.IsTrue(presentation.IsConfigured);

            var pool = presentation.VfxPool;
            Assert.IsTrue(pool.IsConfigured);
            Assert.GreaterOrEqual(pool.PrewarmedCount, 18);

            Assert.IsTrue(presentation.AudioFeedback.IsConfigured);
            Assert.AreEqual(3, presentation.AudioFeedback.AmbientSourceCount);
            Assert.IsTrue(presentation.CameraPolish.IsConfigured);

            Assert.IsTrue(GameObject.Find("H7_WorldArt")
                .GetComponent<H7WorldArtLayer>().IsConfigured);
            Assert.IsTrue(GameObject.Find("H7_StatusPanel")
                .GetComponent<H7StatusHud>().IsConfigured);

            AssertCharacter("Player", "H7_Player_Visual", "Idle", "Walk", "Attack");
            AssertCharacter("Mordeluz", "Mordeluz_H7_Visual", "Idle", "Walk", "Attack");
            AssertCharacter("Mordeluz_2", "Mordeluz_2_H7_Visual", "Idle", "Walk", "Attack");
            AssertCharacter("Mordeluz_3", "Mordeluz_3_H7_Visual", "Idle", "Walk", "Attack");
            AssertCharacter("Mordeluz_Resonante", "Mordeluz_Resonante_H7_Visual",
                "Idle", "Walk", "Telegraph");
            AssertCharacter("Nara_Velaquieta", "H7_Nara_Visual", "Idle", "Talk", "MissionReady");
        }

        private static void AssertCharacter(string objectName, string visualName,
            params string[] requiredStates)
        {
            var owner = objectName == "Player"
                ? GameObject.FindWithTag("Player")
                : GameObject.Find(objectName);
            var view = owner.GetComponent<H7CharacterView>();
            Assert.IsTrue(view.IsPresentationReady, objectName + " should have a sprite and Animator.");
            Assert.IsNotNull(owner.transform.Find(visualName));

            foreach (var state in requiredStates)
            {
                Assert.IsTrue(view.Animator.HasState(0,
                    Animator.StringToHash("Base Layer." + state)),
                    objectName + " is missing animation state " + state + ".");
            }
        }
    }
}
