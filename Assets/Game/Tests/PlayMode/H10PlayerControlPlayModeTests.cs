using System.Collections;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Domain.Combat;
using Lumbre.Game.Domain.Constants;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H10PlayerControlPlayModeTests
    {
        [UnityTest]
        public IEnumerator PlayerExposesResponsiveLocomotionConfiguration()
        {
            yield return LoadSlice();
            var controller = GetPlayer().GetComponent<H3PlayerController>();

            Assert.That(controller.MaxSpeed, Is.EqualTo(2.4f).Within(0.01f));
            Assert.That(controller.Acceleration, Is.GreaterThan(0f));
            Assert.That(controller.Deceleration, Is.GreaterThan(controller.Acceleration));
        }

        [UnityTest]
        public IEnumerator PlayerMovesAlongLogicalEast()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var start = player.transform.position;
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return HoldStick(gamepad, Vector2.right, 0.25f);
                var delta = (Vector2)player.transform.position - (Vector2)start;
                Assert.That(Vector2.Dot(delta, H3PlayerController.ToWorldVector(1f, 0f)),
                    Is.GreaterThan(0.05f));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovesAlongLogicalWest()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var start = player.transform.position;
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return HoldStick(gamepad, Vector2.left, 0.25f);
                var delta = (Vector2)player.transform.position - (Vector2)start;
                Assert.That(Vector2.Dot(delta, H3PlayerController.ToWorldVector(-1f, 0f)),
                    Is.GreaterThan(0.05f));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovesAlongLogicalNorth()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var start = player.transform.position;
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return HoldStick(gamepad, Vector2.up, 0.25f);
                var delta = (Vector2)player.transform.position - (Vector2)start;
                Assert.That(Vector2.Dot(delta, H3PlayerController.ToWorldVector(0f, 1f)),
                    Is.GreaterThan(0.05f));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovesAlongLogicalSouth()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var start = player.transform.position;
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return HoldStick(gamepad, Vector2.down, 0.25f);
                var delta = (Vector2)player.transform.position - (Vector2)start;
                Assert.That(Vector2.Dot(delta, H3PlayerController.ToWorldVector(0f, -1f)),
                    Is.GreaterThan(0.05f));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator DiagonalMovementNeverExceedsConfiguredSpeed()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return HoldStick(gamepad, new Vector2(1f, 1f), 0.4f);
                var controller = player.GetComponent<H3PlayerController>();
                Assert.That(controller.CurrentWorldVelocity.magnitude,
                    Is.LessThanOrEqualTo(controller.MaxSpeed + 0.001f));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator ReleasingTheStickDeceleratesToRest()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.QueueDeltaStateEvent(gamepad.leftStick, Vector2.right);
                InputSystem.Update();
                yield return new WaitForSeconds(0.2f);
                var movingSpeed = player.GetComponent<H3PlayerController>().CurrentWorldVelocity.magnitude;
                InputSystem.QueueDeltaStateEvent(gamepad.leftStick, Vector2.zero);
                InputSystem.Update();
                yield return new WaitForSeconds(0.12f);
                Assert.That(player.GetComponent<H3PlayerController>().CurrentWorldVelocity.magnitude,
                    Is.LessThan(movingSpeed));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator LookDirectionPersistsAfterInputRelease()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return HoldStick(gamepad, Vector2.up, 0.2f);
                var controller = player.GetComponent<H3PlayerController>();
                var look = controller.CurrentLookDirection;
                Assert.That(Vector2.Dot(look, H3PlayerController.ToWorldVector(0f, 1f).normalized),
                    Is.GreaterThan(0.95f));
                Assert.That(controller.CurrentLookDirection, Is.EqualTo(look));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator PlayerCollisionSetupUsesContinuousRigidbodyAndCollider()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var body = player.GetComponent<Rigidbody2D>();
            var collider = player.GetComponent<CircleCollider2D>();

            Assert.That(body.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode2D.Continuous));
            Assert.IsTrue(collider.enabled);
            Assert.IsTrue(collider.isTrigger == false);
        }

        [UnityTest]
        public IEnumerator TouchControlsUseTheOfficialActionPaths()
        {
            yield return LoadSlice();
            Assert.AreEqual("<Gamepad>/buttonSouth", ReadControlPath("H4_AttackButton"));
            Assert.AreEqual("<Gamepad>/buttonEast", ReadControlPath("H4B_DefenseButton"));
            Assert.AreEqual("<Gamepad>/buttonWest", ReadControlPath("H4B_AreaButton"));
            Assert.AreEqual("<Gamepad>/buttonNorth", ReadControlPath("H5_InteractButton"));
            Assert.AreEqual("<Gamepad>/leftShoulder", ReadControlPath("H5_EquipButton"));
        }

        [UnityTest]
        public IEnumerator TouchControlsExposePressedFeedback()
        {
            yield return LoadSlice();
            foreach (var name in new[]
            {
                "H4_AttackButton", "H4B_DefenseButton", "H4B_AreaButton",
                "H5_InteractButton", "H5_EquipButton"
            })
            {
                Assert.IsNotNull(GameObject.Find(name)?.GetComponent<H10TouchButtonFeedback>(), name);
            }
        }

        [UnityTest]
        public IEnumerator KeyboardAndGamepadAttackDamagesTheTarget()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var target = PrepareTarget(player, "Mordeluz");
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return PressGamepadButton(gamepad, GamepadButton.South);
                Assert.That(target.CurrentHealth, Is.LessThan(target.MaxHealth));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator TouchAttackUsesTheSameActionAsGamepadAttack()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var target = PrepareTarget(player, "Mordeluz");

            yield return PressTouchButton("H4_AttackButton");

            Assert.That(target.CurrentHealth, Is.LessThan(target.MaxHealth));
        }

        [UnityTest]
        public IEnumerator OneTouchPressProducesOneBasicAttack()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var target = PrepareTarget(player, "Mordeluz");

            yield return PressTouchButton("H4_AttackButton");

            Assert.AreEqual(target.MaxHealth - ProjectConstants.PlayerBasicAttackDamage,
                target.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator TouchDefenseActivatesThePlayerDefenseState()
        {
            yield return LoadSlice();
            var abilities = GetPlayer().GetComponent<H4BPlayerAbilityController>();

            yield return PressTouchButton("H4B_DefenseButton");

            Assert.IsTrue(abilities.IsDefenseActive);
        }

        [UnityTest]
        public IEnumerator DefenseReducesDamageDuringItsActiveWindow()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var health = player.GetComponent<H4CombatHealth>();
            var abilities = player.GetComponent<H4BPlayerAbilityController>();
            var before = health.CurrentHealth;

            Assert.IsTrue(abilities.TryActivateDefense().Succeeded);
            health.ReceiveDamage(new CombatDamage(20, "h10-defense"));

            Assert.AreEqual(before - 7, health.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator AreaTouchAttackSpendsHeatAndDamagesNearbyEnemy()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var target = PrepareTarget(player, "Mordeluz");
            var abilities = player.GetComponent<H4BPlayerAbilityController>();
            abilities.AddHeat(100);

            yield return PressTouchButton("H4B_AreaButton");

            Assert.That(abilities.CurrentHeat, Is.EqualTo(50));
            Assert.That(target.CurrentHealth, Is.EqualTo(15));
        }

        [UnityTest]
        public IEnumerator AreaAttackDoesNotDamageTargetsOutsideItsRadius()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var target = PrepareTarget(player, "Mordeluz");
            target.transform.position = player.transform.position + Vector3.right * 5f;
            Physics2D.SyncTransforms();
            var abilities = player.GetComponent<H4BPlayerAbilityController>();
            abilities.AddHeat(100);

            Assert.IsTrue(abilities.TryAreaAttack().Succeeded);
            yield return null;

            Assert.AreEqual(target.MaxHealth, target.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator PauseBlocksMovementAndCombatActions()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var pause = GameObject.Find("H8_UXRoot").GetComponent<H8PauseController>();
            var combat = player.GetComponent<H4PlayerCombatController>();
            var abilities = player.GetComponent<H4BPlayerAbilityController>();

            pause.TogglePause();
            Assert.IsTrue(pause.IsPaused);
            Assert.AreEqual(AttackResultCode.InvalidTarget, combat.TryBasicAttack().Code);
            Assert.AreEqual(AbilityResultCode.InvalidState, abilities.TryActivateDefense().Code);
        }

        [UnityTest]
        public IEnumerator ResumeRestoresTheActionBoundary()
        {
            yield return LoadSlice();
            var pause = GameObject.Find("H8_UXRoot").GetComponent<H8PauseController>();
            var abilities = GetPlayer().GetComponent<H4BPlayerAbilityController>();

            pause.TogglePause();
            pause.Resume();

            Assert.IsFalse(pause.IsPaused);
            Assert.IsTrue(abilities.TryActivateDefense().Succeeded);
        }

        [UnityTest]
        public IEnumerator InteractionAndEquipControlsRemainConnected()
        {
            yield return LoadSlice();
            var player = GetPlayer();

            Assert.IsNotNull(player.GetComponent<Lumbre.Game.Client.Missions.H5PlayerMissionController>());
            Assert.IsNotNull(GameObject.Find("Nara_Velaquieta"));
            Assert.IsNotNull(GameObject.Find("H5_InteractButton"));
            Assert.IsNotNull(GameObject.Find("H5_EquipButton"));
        }

        [UnityTest]
        public IEnumerator EnemyControllersRemainConfiguredForExistingCombatFlow()
        {
            yield return LoadSlice();

            Assert.IsNotNull(GameObject.Find("Mordeluz").GetComponent<MordeluzController>());
            Assert.IsNotNull(GameObject.Find("Mordeluz_2").GetComponent<MordeluzController>());
            Assert.IsNotNull(GameObject.Find("Mordeluz_3").GetComponent<MordeluzController>());
            Assert.IsNotNull(GameObject.Find("Mordeluz_Resonante")
                .GetComponent<MordeluzResonanteController>());
        }

        [UnityTest]
        public IEnumerator BootstrapStillActivatesTheVerticalSlice()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(ProjectConstants.BootstrapSceneName, LoadSceneMode.Single);
            yield return new WaitForSecondsRealtime(0.5f);
            var waits = 0;
            while (SceneManager.GetActiveScene().name != ProjectConstants.VerticalSliceSceneName
                && waits++ < 20)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            Assert.AreEqual(ProjectConstants.VerticalSliceSceneName, SceneManager.GetActiveScene().name);
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator SafeAreaContainsTheTouchSurface()
        {
            yield return LoadSlice();
            var safeArea = GameObject.Find("H9_SafeAreaRoot")?.GetComponent<H9SafeAreaLayout>();
            var canvas = GameObject.Find("H3_MobileControls")?.GetComponent<Canvas>();

            Assert.IsNotNull(safeArea);
            Assert.IsTrue(safeArea.IsConfigured);
            Assert.IsNotNull(canvas);
            Assert.IsTrue(canvas.renderMode != RenderMode.WorldSpace);
        }

        private static IEnumerator LoadSlice()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(ProjectConstants.VerticalSliceSceneName, LoadSceneMode.Single);
            yield return null;
            yield return new WaitForFixedUpdate();
        }

        private static GameObject GetPlayer()
        {
            var player = GameObject.FindWithTag("Player");
            Assert.IsNotNull(player);
            return player;
        }

        private static H4CombatHealth PrepareTarget(GameObject player, string targetName)
        {
            DisableEnemyAi();
            var target = GameObject.Find(targetName).GetComponent<H4CombatHealth>();
            var position = target.transform.position + Vector3.left * 0.85f;
            var body = player.GetComponent<Rigidbody2D>();
            body.position = position;
            player.transform.position = position;
            Physics2D.SyncTransforms();
            return target;
        }

        private static void DisableEnemyAi()
        {
            foreach (var ai in Object.FindObjectsByType<MordeluzController>(FindObjectsSortMode.None))
            {
                ai.enabled = false;
            }

            foreach (var ai in Object.FindObjectsByType<MordeluzResonanteController>(FindObjectsSortMode.None))
            {
                ai.enabled = false;
            }
        }

        private static string ReadControlPath(string objectName)
        {
            var button = GameObject.Find(objectName)?.GetComponent(
                "UnityEngine.InputSystem.OnScreen.OnScreenButton");
            return button?.GetType().GetProperty("controlPath")?.GetValue(button) as string;
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

        private static IEnumerator PressGamepadButton(Gamepad gamepad, GamepadButton button)
        {
            InputSystem.QueueStateEvent(gamepad,
                new GamepadState().WithButton(button, true));
            InputSystem.Update();
            yield return null;
            InputSystem.QueueStateEvent(gamepad,
                new GamepadState().WithButton(button, false));
            InputSystem.Update();
            yield return null;
        }

        private static IEnumerator PressTouchButton(string objectName)
        {
            var eventSystem = EventSystem.current;
            Assert.IsNotNull(eventSystem);
            var button = GameObject.Find(objectName)?.GetComponent(
                "UnityEngine.InputSystem.OnScreen.OnScreenButton");
            var down = button as IPointerDownHandler;
            var up = button as IPointerUpHandler;
            Assert.IsNotNull(down, objectName);
            Assert.IsNotNull(up, objectName);
            var pointer = new PointerEventData(eventSystem);

            down.OnPointerDown(pointer);
            InputSystem.Update();
            yield return null;
            up.OnPointerUp(pointer);
            InputSystem.Update();
            yield return null;
        }
    }
}
