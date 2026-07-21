using System.Collections;
using System.IO;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Missions;
using Lumbre.Game.Client.Progression;
using Lumbre.Game.Domain.Missions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H5MissionRewardPlayModeTests
    {
        private string _savePath;
        private string _temporarySavePath;
        private bool _hadExistingSave;
        private string _existingSave;

        [SetUp]
        public void SetUp()
        {
            _savePath = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                H6ProgressionRuntime.DefaultSaveFileName);
            _temporarySavePath = _savePath + ".tmp";
            _hadExistingSave = File.Exists(_savePath);
            _existingSave = _hadExistingSave ? File.ReadAllText(_savePath) : string.Empty;
            DeleteSaveFiles();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteSaveFiles();
            if (_hadExistingSave)
            {
                File.WriteAllText(_savePath, _existingSave);
            }
        }

        [UnityTest]
        public IEnumerator PlayerCompletesNaraMissionAndEquipsOneReward()
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
            var runtime = GameObject.Find("H5_MissionRuntime").GetComponent<H5MissionRuntime>();
            var nara = GameObject.Find("Nara_Velaquieta").GetComponent<H5NaraController>();

            Assert.IsNotNull(player);
            Assert.IsNotNull(playerBody);
            Assert.IsNotNull(playerHealth);
            Assert.IsNotNull(playerCombat);
            Assert.IsNotNull(abilities);
            Assert.IsNotNull(missionPlayer);
            Assert.IsNotNull(runtime);
            Assert.IsNotNull(nara);
            Assert.AreEqual(MissionState.Available, runtime.Mission.State);

            playerBody.position = nara.transform.position;
            player.transform.position = playerBody.position;
            Physics2D.SyncTransforms();
            Assert.IsTrue(nara.IsInRange(player.transform));
            Assert.IsTrue(missionPlayer.TryInteract().Succeeded,
                "The player should accept Nara's mission by proximity.");
            Assert.AreEqual(MissionState.Active, runtime.Mission.State);

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
            Assert.AreEqual(15, common[0].CurrentHealth);
            Assert.AreEqual(15, common[1].CurrentHealth);
            Assert.AreEqual(15, common[2].CurrentHealth);

            foreach (var target in common)
            {
                while (target.IsAlive)
                {
                    Assert.IsTrue(playerCombat.TryBasicAttack().Succeeded);
                    yield return new WaitForSeconds(0.65f);
                }
            }

            Assert.AreEqual(3, runtime.Mission.Snapshot.MordeluzDefeated);
            Assert.AreEqual(MissionState.Active, runtime.Mission.State);

            var resonant = GameObject.Find("Mordeluz_Resonante");
            var resonantHealth = resonant.GetComponent<H4CombatHealth>();
            resonant.GetComponent<MordeluzResonanteController>().enabled = false;
            resonant.transform.position = clusterCenter + new Vector2(0.5f, 0f);
            Physics2D.SyncTransforms();

            while (resonantHealth.IsAlive)
            {
                Assert.IsTrue(playerCombat.TryBasicAttack().Succeeded,
                    "The player should defeat the Resonant Mordeluz with the prototype attack.");
                yield return new WaitForSeconds(0.65f);
            }

            Assert.AreEqual(1, runtime.Mission.Snapshot.ResonantDefeated);
            Assert.AreEqual(MissionState.ReadyToTurnIn, runtime.Mission.State);

            playerBody.position = nara.transform.position;
            player.transform.position = playerBody.position;
            Physics2D.SyncTransforms();
            var delivery = missionPlayer.TryInteract();
            Assert.IsTrue(delivery.Succeeded);
            Assert.AreEqual(MissionOperationCode.RewardDelivered, delivery.Code);
            Assert.AreEqual(MissionState.Completed, runtime.Mission.State);
            Assert.AreEqual(1, runtime.Inventory.Count);
            Assert.IsTrue(runtime.Inventory.Contains(H5MissionModel.RewardItemId));

            var duplicateDelivery = missionPlayer.TryInteract();
            Assert.AreEqual(MissionOperationCode.Completed, duplicateDelivery.Code);
            Assert.AreEqual(1, runtime.Inventory.Count);

            var equip = missionPlayer.TryEquipReward();
            Assert.IsTrue(equip.Succeeded);
            Assert.IsTrue(runtime.Equipment.EquippedItem.HasValue);
            Assert.AreEqual(H5MissionModel.RewardItemId,
                runtime.Equipment.EquippedItem.Value.ItemId);
            Assert.IsTrue(runtime.Inventory.Contains(H5MissionModel.RewardItemId),
                "Equipping must not remove the reward from the six-slot inventory.");
        }

        private static void DisableCommonAi()
        {
            GameObject.Find("Mordeluz").GetComponent<MordeluzController>().enabled = false;
            GameObject.Find("Mordeluz_2").GetComponent<MordeluzController>().enabled = false;
            GameObject.Find("Mordeluz_3").GetComponent<MordeluzController>().enabled = false;
        }

        private void DeleteSaveFiles()
        {
            if (File.Exists(_savePath))
            {
                File.Delete(_savePath);
            }

            if (File.Exists(_temporarySavePath))
            {
                File.Delete(_temporarySavePath);
            }
        }
    }
}
