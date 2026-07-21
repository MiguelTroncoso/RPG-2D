using System.Collections;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Domain.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H4BAbilitiesElitePlayModeTests
    {
        [UnityTest]
        public IEnumerator PlayerUsesAbilitiesDefeatsThreeMordeluzAndResonantElite()
        {
            SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
            yield return null;
            yield return new WaitForFixedUpdate();

            var player = GameObject.FindWithTag("Player");
            var playerBody = player.GetComponent<Rigidbody2D>();
            var playerHealth = player.GetComponent<H4CombatHealth>();
            var playerCombat = player.GetComponent<H4PlayerCombatController>();
            var abilities = player.GetComponent<H4BPlayerAbilityController>();
            Assert.IsNotNull(player);
            Assert.IsNotNull(playerBody);
            Assert.IsNotNull(playerHealth);
            Assert.IsNotNull(playerCombat);
            Assert.IsNotNull(abilities);

            var first = GameObject.Find("Mordeluz").GetComponent<H4CombatHealth>();
            var second = GameObject.Find("Mordeluz_2").GetComponent<H4CombatHealth>();
            var third = GameObject.Find("Mordeluz_3").GetComponent<H4CombatHealth>();
            var resonant = GameObject.Find("Mordeluz_Resonante");
            var resonantHealth = resonant.GetComponent<H4CombatHealth>();
            var resonantController = resonant.GetComponent<MordeluzResonanteController>();
            var telegraph = resonant.GetComponent<H4BWaveTelegraph>();
            Assert.IsNotNull(resonantController);
            Assert.IsNotNull(telegraph);

            foreach (var target in new[] { "Mordeluz", "Mordeluz_2", "Mordeluz_3" })
            {
                GameObject.Find(target).GetComponent<MordeluzController>().enabled = false;
            }

            playerHealth.RestoreToFull();
            abilities.AddHeat(100);
            var healthBeforeDefense = playerHealth.CurrentHealth;
            Assert.IsTrue(abilities.TryActivateDefense().Succeeded);
            playerHealth.ReceiveDamage(new CombatDamage(20, "h4b-defense-test"));
            Assert.AreEqual(healthBeforeDefense - 7, playerHealth.CurrentHealth,
                "Defense should apply the configured damage reduction during its duration.");

            var clusterCenter = playerBody.position;
            var common = new[] { first, second, third };
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
            var areaResult = abilities.TryAreaAttack();
            Assert.IsTrue(areaResult.Succeeded, "Area attack should spend the available Heat.");
            yield return null;
            Assert.AreEqual(15, first.CurrentHealth);
            Assert.AreEqual(15, second.CurrentHealth);
            Assert.AreEqual(15, third.CurrentHealth);

            foreach (var target in common)
            {
                while (target.IsAlive)
                {
                    var attack = playerCombat.TryBasicAttack();
                    Assert.IsTrue(attack.Succeeded, "The player should finish each area-damaged target.");
                    yield return new WaitForSeconds(0.65f);
                }
            }

            Assert.IsFalse(first.IsAlive);
            Assert.IsFalse(second.IsAlive);
            Assert.IsFalse(third.IsAlive);

            playerHealth.RestoreToFull();
            playerBody.position = resonant.transform.position + Vector3.left * 0.85f;
            player.transform.position = playerBody.position;
            Physics2D.SyncTransforms();

            var elapsed = 0f;
            while (!resonantController.IsWaveTelegraphActive && elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(resonantController.IsWaveTelegraphActive);
            Assert.IsTrue(telegraph.IsVisible, "The wave must show a visible danger zone during anticipation.");
            var healthBeforeWave = playerHealth.CurrentHealth;
            yield return new WaitForSeconds(0.9f);
            Assert.Less(playerHealth.CurrentHealth, healthBeforeWave,
                "The Resonant wave should damage targets that remain inside its danger zone.");
            Assert.IsFalse(telegraph.IsVisible);

            while (resonantHealth.IsAlive)
            {
                var attack = playerCombat.TryBasicAttack();
                Assert.IsTrue(attack.Succeeded, "The player should be able to defeat the Resonant Mordeluz.");
                yield return new WaitForSeconds(0.65f);
            }

            Assert.IsFalse(resonantHealth.IsAlive);
            Assert.AreEqual(MordeluzAiState.Dead, resonantController.CurrentState);
        }
    }
}
