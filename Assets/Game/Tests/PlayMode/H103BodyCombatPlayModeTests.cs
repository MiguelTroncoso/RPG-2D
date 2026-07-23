using System.Collections;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Domain.Combat;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lumbre.Game.Tests
{
    public sealed class H103BodyCombatPlayModeTests
    {
        // ---- Locomotion body rig ----

        [UnityTest]
        public IEnumerator PlayerBodyRigIsAssembledAndReady()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var rig = player.GetComponent<H103BodyRigView>();

            Assert.IsNotNull(rig, "player must carry the H10.3 body rig view");
            Assert.IsTrue(rig.IsRigReady, "every rig limb reference must resolve");

            var visual = player.transform.Find("H7_Player_Visual");
            var baseRenderer = visual.GetComponent<SpriteRenderer>();
            Assert.IsFalse(baseRenderer.enabled,
                "the single-illustration renderer is replaced by the rig");
            Assert.IsNotNull(baseRenderer.sprite,
                "the base sprite is retained for the H7 presentation contract");
            // The rig is a sibling of the H7 visual (direct child of the
            // player) so the H7 animator's scale curves never touch the rig.
            Assert.IsNotNull(player.transform.Find(H103BodyRigView.RigRootName));
        }

        [UnityTest]
        public IEnumerator WalkingAdvancesTheStrideWhileIdleDoesNot()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var rig = player.GetComponent<H103BodyRigView>();
            yield return null;
            yield return null;
            var idlePhase = rig.PoseModel.StridePhase;
            yield return null;
            Assert.AreEqual(idlePhase, rig.PoseModel.StridePhase, 1e-4f,
                "an idle player must not accumulate stride phase");

            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.QueueDeltaStateEvent(gamepad.leftStick, Vector2.right);
                InputSystem.Update();
                yield return new WaitForSeconds(0.3f);
                Assert.Greater(rig.PoseModel.StridePhase, idlePhase + 0.1f,
                    "physical travel must drive the stride phase forward");
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator FacingStaysStableAcrossDiagonalInput()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var rig = player.GetComponent<H103BodyRigView>();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.QueueDeltaStateEvent(gamepad.leftStick, Vector2.right);
                InputSystem.Update();
                yield return new WaitForSeconds(0.2f);
                var facing = rig.FacingSign;
                Assert.AreEqual(1f, facing, "moving right faces right");

                // A near-vertical diagonal must not flip the facing.
                InputSystem.QueueDeltaStateEvent(gamepad.leftStick,
                    new Vector2(0.05f, 1f));
                InputSystem.Update();
                yield return new WaitForSeconds(0.2f);
                Assert.AreEqual(facing, rig.FacingSign,
                    "a small horizontal component must not flip facing");
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        // ---- Attack phases: damage at the visible impact only ----

        [UnityTest]
        public IEnumerator AttackProgressesThroughVisiblePhases()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            PrepareTarget(player, "Mordeluz");
            var combat = player.GetComponent<H4PlayerCombatController>();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return PressGamepadButton(gamepad, GamepadButton.South);
                Assert.AreNotEqual(AttackPhase.Idle, combat.CurrentAttackPhase,
                    "the attack must start with a windup phase");

                var seen = new System.Collections.Generic.HashSet<AttackPhase>();
                var deadline = Time.time + combat.AttackSequenceSeconds + 0.3f;
                while (Time.time < deadline)
                {
                    seen.Add(combat.CurrentAttackPhase);
                    yield return null;
                }

                Assert.IsTrue(seen.Contains(AttackPhase.Windup));
                Assert.IsTrue(seen.Contains(AttackPhase.Strike));
                Assert.IsTrue(seen.Contains(AttackPhase.Impact));
                Assert.IsTrue(seen.Contains(AttackPhase.Recovery));
                Assert.AreEqual(AttackPhase.Idle, combat.CurrentAttackPhase,
                    "the sequence must return to idle");
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator DamageLandsAtImpactNeverAtButtonPress()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var target = PrepareTarget(player, "Mordeluz");
            var combat = player.GetComponent<H4PlayerCombatController>();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return PressGamepadButton(gamepad, GamepadButton.South);
                // Immediately after the press the enemy must be unharmed.
                Assert.AreEqual(target.MaxHealth, target.CurrentHealth,
                    "no damage may occur during windup/strike");

                var damaged = false;
                var deadline = Time.time + combat.AttackSequenceSeconds + 0.3f;
                while (Time.time < deadline && !damaged)
                {
                    damaged = target.CurrentHealth < target.MaxHealth;
                    yield return null;
                }

                Assert.IsTrue(damaged, "damage must land during the impact");
                Assert.AreEqual(target.MaxHealth - ProjectConstants.PlayerBasicAttackDamage,
                    target.CurrentHealth, "exactly one hit of damage");
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator AttackReleasesTheActionStateAfterRecovery()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            PrepareTarget(player, "Mordeluz");
            var combat = player.GetComponent<H4PlayerCombatController>();
            var actionState = player.GetComponent<H10PlayerActionStateController>();
            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                yield return PressGamepadButton(gamepad, GamepadButton.South);
                Assert.AreEqual(PlayerActionState.Attacking, actionState.CurrentState);

                var deadline = Time.time + combat.AttackSequenceSeconds + 0.3f;
                while (combat.IsAttackSequenceActive && Time.time < deadline)
                {
                    yield return null;
                }

                yield return null;
                Assert.AreNotEqual(PlayerActionState.Attacking, actionState.CurrentState,
                    "the action state must not stay locked in Attacking");
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        // ---- Enemy hit reaction ----

        [UnityTest]
        public IEnumerator EnemyStaggersVisiblyWhenItTakesDamage()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var enemyObject = GameObject.Find("Mordeluz");
            var enemy = enemyObject.GetComponent<MordeluzController>();
            var enemyHealth = enemyObject.GetComponent<H4CombatHealth>();
            Assert.IsFalse(enemy.IsStaggered);

            enemyHealth.ReceiveDamage(new CombatDamage(10, "test-hit"));
            Assert.IsTrue(enemy.IsStaggered, "the enemy must react on receiving damage");

            yield return new WaitForSeconds(ProjectConstants.MordeluzStaggerSeconds + 0.1f);
            Assert.IsFalse(enemy.IsStaggered,
                "the stagger must clear and not leave the enemy frozen");
            _ = player;
        }

        [UnityTest]
        public IEnumerator EnemyIsPushedAwayFromThePlayerOnHit()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var enemyObject = GameObject.Find("Mordeluz");
            var enemy = enemyObject.GetComponent<MordeluzController>();
            enemy.Target = player.transform;
            // Place the enemy to the right of the player so knockback is +x.
            enemyObject.transform.position = player.transform.position + Vector3.right;
            Physics2D.SyncTransforms();
            var before = enemyObject.transform.position.x;

            enemyObject.GetComponent<H4CombatHealth>()
                .ReceiveDamage(new CombatDamage(10, "test-hit"));
            yield return new WaitForSeconds(ProjectConstants.MordeluzKnockbackSeconds + 0.1f);

            Assert.Greater(enemyObject.transform.position.x, before,
                "the hit must push the enemy away from the player");
        }

        // ---- DEF / HAB cleanup ----

        [UnityTest]
        public IEnumerator DefenseCleansUpAndDoesNotLockTheActionState()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var abilities = player.GetComponent<H4BPlayerAbilityController>();
            var actionState = player.GetComponent<H10PlayerActionStateController>();

            Assert.IsTrue(abilities.TryActivateDefense().Succeeded);
            Assert.AreNotEqual(PlayerActionState.Defending, actionState.CurrentState,
                "programmatic defense must not leave the action state locked");

            // Wait for the defense window to elapse; the modifier must clear.
            yield return new WaitForSeconds(ProjectConstants.PlayerDefenseDuration + 0.2f);
            Assert.IsFalse(abilities.IsDefenseActive,
                "defense must expire instead of remaining active");
            var health = player.GetComponent<H4CombatHealth>();
            var full = health.CurrentHealth;
            health.ReceiveDamage(new CombatDamage(20, "post-defense"));
            Assert.AreEqual(full - 20, health.CurrentHealth,
                "the defense damage modifier must be cleaned up after it expires");
        }

        [UnityTest]
        public IEnumerator DefenseInputReleasesTheActionStateForFollowUpActions()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var abilities = player.GetComponent<H4BPlayerAbilityController>();
            var actionState = player.GetComponent<H10PlayerActionStateController>();

            yield return PressTouchButton("H4B_DefenseButton");
            Assert.IsTrue(abilities.IsDefenseActive);
            // The gate must be free right after the input frame so the player
            // is never stuck unable to act.
            Assert.AreEqual(PlayerActionState.Idle, actionState.CurrentState);
        }

        [UnityTest]
        public IEnumerator AreaAttackReleasesTheActionState()
        {
            yield return LoadSlice();
            var player = GetPlayer();
            var abilities = player.GetComponent<H4BPlayerAbilityController>();
            var actionState = player.GetComponent<H10PlayerActionStateController>();
            abilities.AddHeat(100);

            Assert.IsTrue(abilities.TryAreaAttack().Succeeded);
            Assert.AreNotEqual(PlayerActionState.AreaAttack, actionState.CurrentState,
                "the area attack must release the action gate");
            yield return null;
        }

        // ---- helpers ----

        private static IEnumerator LoadSlice()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(ProjectConstants.VerticalSliceSceneName,
                LoadSceneMode.Single);
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
            foreach (var ai in Object.FindObjectsByType<MordeluzController>(
                FindObjectsSortMode.None))
            {
                ai.enabled = false;
            }

            var target = GameObject.Find(targetName).GetComponent<H4CombatHealth>();
            var position = target.transform.position + Vector3.left * 0.85f;
            var body = player.GetComponent<Rigidbody2D>();
            body.position = position;
            player.transform.position = position;
            Physics2D.SyncTransforms();
            return target;
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
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            Assert.IsNotNull(eventSystem);
            var button = GameObject.Find(objectName)?.GetComponent(
                "UnityEngine.InputSystem.OnScreen.OnScreenButton");
            var down = button as UnityEngine.EventSystems.IPointerDownHandler;
            var up = button as UnityEngine.EventSystems.IPointerUpHandler;
            Assert.IsNotNull(down, objectName);
            Assert.IsNotNull(up, objectName);
            var pointer = new UnityEngine.EventSystems.PointerEventData(eventSystem);

            down.OnPointerDown(pointer);
            InputSystem.Update();
            yield return null;
            up.OnPointerUp(pointer);
            InputSystem.Update();
            yield return null;
        }
    }
}
