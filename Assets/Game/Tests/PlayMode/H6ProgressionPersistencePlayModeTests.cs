using System.Collections;
using System.IO;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Missions;
using Lumbre.Game.Client.Progression;
using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Missions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H6ProgressionPersistencePlayModeTests
    {
        [UnityTest]
        public IEnumerator VerticalSliceReachesLevelTwoAndRestoresEverythingAfterReload()
        {
            var savePath = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                H6ProgressionRuntime.DefaultSaveFileName);
            var temporaryPath = savePath + ".tmp";
            var hadExistingSave = File.Exists(savePath);
            var existingSave = hadExistingSave ? File.ReadAllText(savePath) : string.Empty;
            DeleteSave(savePath, temporaryPath);

            try
            {
                SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
                yield return null;
                yield return new WaitForFixedUpdate();

                var player = GameObject.FindWithTag("Player");
                var playerBody = player.GetComponent<Rigidbody2D>();
                var playerHealth = player.GetComponent<H4CombatHealth>();
                var playerCombat = player.GetComponent<H4PlayerCombatController>();
                var abilities = player.GetComponent<H4BPlayerAbilityController>();
                var missionPlayer = player.GetComponent<H5PlayerMissionController>();
                var missionRuntime = GameObject.Find("H5_MissionRuntime")
                    .GetComponent<H5MissionRuntime>();
                var progressionRuntime = GameObject.Find("H6_ProgressionRuntime")
                    .GetComponent<H6ProgressionRuntime>();
                var nara = GameObject.Find("Nara_Velaquieta")
                    .GetComponent<H5NaraController>();

                Assert.AreEqual(Lumbre.Game.Application.Persistence.SaveLoadStatus.NewGame,
                    progressionRuntime.LastLoadStatus);
                Assert.AreEqual(1, progressionRuntime.Progression.Snapshot.Level);
                Assert.IsTrue(missionPlayer.TryInteract().Succeeded);
                Assert.AreEqual(MissionState.Active, missionRuntime.Mission.State);

                DisableCommonAi();
                playerHealth.RestoreToFull();
                abilities.AddHeat(100);
                var common = new[]
                {
                    GameObject.Find("Mordeluz").GetComponent<H4CombatHealth>(),
                    GameObject.Find("Mordeluz_2").GetComponent<H4CombatHealth>(),
                    GameObject.Find("Mordeluz_3").GetComponent<H4CombatHealth>()
                };
                var clusterCenter = playerBody.position;
                var offsets = new[]
                {
                    Vector2.zero,
                    new Vector2(0.65f, 0.1f),
                    new Vector2(-0.65f, -0.1f)
                };
                for (var index = 0; index < common.Length; index++)
                {
                    common[index].transform.position = clusterCenter + offsets[index];
                }

                Physics2D.SyncTransforms();
                Assert.IsTrue(abilities.TryAreaAttack().Succeeded);
                yield return null;
                foreach (var target in common)
                {
                    while (target.IsAlive)
                    {
                        Assert.IsTrue(playerCombat.TryBasicAttack().Succeeded);
                        yield return new WaitForSeconds(0.65f);
                    }
                }

                var resonant = GameObject.Find("Mordeluz_Resonante");
                var resonantHealth = resonant.GetComponent<H4CombatHealth>();
                resonant.GetComponent<MordeluzResonanteController>().enabled = false;
                resonant.transform.position = clusterCenter + new Vector2(0.5f, 0f);
                Physics2D.SyncTransforms();
                while (resonantHealth.IsAlive)
                {
                    Assert.IsTrue(playerCombat.TryBasicAttack().Succeeded);
                    yield return new WaitForSeconds(0.65f);
                }

                Assert.AreEqual(60, progressionRuntime.Progression.Snapshot.TotalExperience);
                Assert.AreEqual(MissionState.ReadyToTurnIn, missionRuntime.Mission.State);

                playerBody.position = nara.transform.position;
                player.transform.position = new Vector3(
                    nara.transform.position.x,
                    nara.transform.position.y,
                    -0.25f);
                Physics2D.SyncTransforms();
                Assert.IsTrue(missionPlayer.TryInteract().Succeeded);
                Assert.AreEqual(MissionState.Completed, missionRuntime.Mission.State);
                Assert.AreEqual(2, progressionRuntime.Progression.Snapshot.Level);
                Assert.AreEqual(100, progressionRuntime.Progression.Snapshot.TotalExperience);
                Assert.IsTrue(missionPlayer.TryEquipReward().Succeeded);
                progressionRuntime.SaveNow();
                Assert.IsTrue(File.Exists(savePath));

                var savedPosition = player.transform.position;
                SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
                yield return null;
                yield return new WaitForFixedUpdate();
                yield return null;

                player = GameObject.FindWithTag("Player");
                missionRuntime = GameObject.Find("H5_MissionRuntime")
                    .GetComponent<H5MissionRuntime>();
                progressionRuntime = GameObject.Find("H6_ProgressionRuntime")
                    .GetComponent<H6ProgressionRuntime>();
                missionPlayer = player.GetComponent<H5PlayerMissionController>();
                nara = GameObject.Find("Nara_Velaquieta").GetComponent<H5NaraController>();

                Assert.AreEqual(Lumbre.Game.Application.Persistence.SaveLoadStatus.Loaded,
                    progressionRuntime.LastLoadStatus);
                Assert.AreEqual(2, progressionRuntime.Progression.Snapshot.Level);
                Assert.AreEqual(100, progressionRuntime.Progression.Snapshot.TotalExperience);
                Assert.AreEqual(MissionState.Completed, missionRuntime.Mission.State);
                Assert.AreEqual(3, missionRuntime.Mission.Snapshot.MordeluzDefeated);
                Assert.AreEqual(1, missionRuntime.Mission.Snapshot.ResonantDefeated);
                Assert.AreEqual(1, missionRuntime.Inventory.Count);
                Assert.AreEqual(H5MissionModel.RewardItemId,
                    missionRuntime.Inventory.GetAt(0).Value.ItemId);
                Assert.AreEqual(H5MissionModel.RewardItemId,
                    missionRuntime.Equipment.EquippedItem.Value.ItemId);
                Assert.That(player.transform.position.x, Is.EqualTo(savedPosition.x).Within(0.01f));
                Assert.That(player.transform.position.y, Is.EqualTo(savedPosition.y).Within(0.01f));
                Assert.IsTrue(nara.IsInRange(player.transform));

                var secondDelivery = missionPlayer.TryInteract();
                Assert.AreEqual(MissionOperationCode.Completed, secondDelivery.Code);
                Assert.AreEqual(1, missionRuntime.Inventory.Count);

                missionRuntime.Publish(new CombatantDefeatedEvent(
                    CombatantKind.Mordeluz, "Mordeluz"));
                Assert.AreEqual(100, progressionRuntime.Progression.Snapshot.TotalExperience);
                Assert.AreEqual(3, missionRuntime.Mission.Snapshot.MordeluzDefeated);
            }
            finally
            {
                DeleteSave(savePath, temporaryPath);
                if (hadExistingSave)
                {
                    File.WriteAllText(savePath, existingSave);
                }
            }
        }

        private static void DisableCommonAi()
        {
            GameObject.Find("Mordeluz").GetComponent<MordeluzController>().enabled = false;
            GameObject.Find("Mordeluz_2").GetComponent<MordeluzController>().enabled = false;
            GameObject.Find("Mordeluz_3").GetComponent<MordeluzController>().enabled = false;
        }

        private static void DeleteSave(string savePath, string temporaryPath)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
