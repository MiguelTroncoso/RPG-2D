using System.Collections;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Constants;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H3PlayerMovementPlayModeTests
    {
        [UnityTest]
        public IEnumerator PlayerCanTravelFromPlazaToCaveAndReturn()
        {
            SceneManager.LoadScene("VerticalSlice", LoadSceneMode.Single);
            yield return null;
            yield return new WaitForFixedUpdate();

            var player = GameObject.FindWithTag("Player");
            Assert.IsNotNull(player);

            var controller = player.GetComponent<H3PlayerController>();
            Assert.IsNotNull(controller);
            var respawnPosition = controller.RespawnPoint.position;
            var gamepad = InputSystem.AddDevice<Gamepad>();

            try
            {
                // Move along logical X to the cave entrance, then step two cells on logical Y
                // to pass the blocked cave cell at (25,10).
                yield return HoldStick(gamepad, Vector2.right, 5.9f);
                yield return HoldStick(gamepad, Vector2.down, 0.7f);
                yield return HoldStick(gamepad, Vector2.right, 1.7f);

                Assert.Greater(player.transform.position.x, 9.5f);
                Assert.Greater(player.transform.position.y, 9.3f);

                // Reverse the same intent sequence back to the approved respawn point.
                yield return HoldStick(gamepad, Vector2.left, 1.7f);
                yield return HoldStick(gamepad, Vector2.up, 0.7f);
                yield return HoldStick(gamepad, Vector2.left, 5.9f);

                Assert.Less(Vector2.Distance(player.transform.position, respawnPosition), 0.65f);
                Assert.AreEqual(ProjectLayers.Player, player.layer);
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        private static IEnumerator HoldStick(Gamepad gamepad, Vector2 value, float seconds)
        {
            InputSystem.QueueDeltaStateEvent(gamepad.leftStick, value);
            InputSystem.Update();
            yield return new WaitForSeconds(seconds);
            InputSystem.QueueDeltaStateEvent(gamepad.leftStick, Vector2.zero);
            InputSystem.Update();
            yield return new WaitForFixedUpdate();
        }
    }
}
