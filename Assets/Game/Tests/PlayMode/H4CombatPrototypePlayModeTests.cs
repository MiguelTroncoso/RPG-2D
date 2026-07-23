using System.Collections;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Domain.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H4CombatPrototypePlayModeTests
    {
        [UnityTest]
        public IEnumerator MordeluzDetectsAttacksAndDiesAfterBasicAttacks()
        {
            SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
            yield return null;
            yield return new WaitForFixedUpdate();

            var player = GameObject.FindWithTag("Player");
            var enemy = GameObject.Find("Mordeluz");
            Assert.IsNotNull(player);
            Assert.IsNotNull(enemy);

            var playerHealth = player.GetComponent<H4CombatHealth>();
            var enemyHealth = enemy.GetComponent<H4CombatHealth>();
            var ai = enemy.GetComponent<MordeluzController>();
            Assert.IsNotNull(playerHealth);
            Assert.IsNotNull(enemyHealth);
            Assert.IsNotNull(ai);
            Assert.IsNotNull(enemy.GetComponent<H4CombatFeedback>());
            Assert.IsNotNull(enemy.GetComponent<AudioSource>());

            var closePosition = enemy.transform.position + Vector3.left * 0.9f;
            var playerBody = player.GetComponent<Rigidbody2D>();
            playerBody.position = new Vector2(closePosition.x, closePosition.y);
            player.transform.position = closePosition;
            Physics2D.SyncTransforms();

            yield return new WaitForSeconds(1.2f);

            Assert.AreEqual(MordeluzAiState.Attack, ai.CurrentState);
            Assert.Less(playerHealth.CurrentHealth, playerHealth.MaxHealth,
                "Mordeluz should deal damage after entering attack range.");

            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return PressAttack(gamepad);
                yield return new WaitForSeconds(0.75f);
                yield return PressAttack(gamepad);
                yield return new WaitForSeconds(0.75f);
                yield return PressAttack(gamepad);
                yield return new WaitForSeconds(0.35f);

                Assert.IsFalse(enemyHealth.IsAlive, "Mordeluz should die at zero health.");
                Assert.AreEqual(MordeluzAiState.Dead, ai.CurrentState);
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        private static IEnumerator PressAttack(Gamepad gamepad)
        {
            InputSystem.QueueStateEvent(gamepad,
                new GamepadState().WithButton(GamepadButton.South, true));
            InputSystem.Update();
            yield return null;
            InputSystem.QueueStateEvent(gamepad,
                new GamepadState().WithButton(GamepadButton.South, false));
            InputSystem.Update();
            // H10.2 resolves input attacks at visual impact. The caller keeps
            // its original cooldown cadence, then waits for the final impact.
            yield return null;
        }
    }
}
