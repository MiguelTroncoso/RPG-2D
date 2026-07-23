using Lumbre.Game.Domain.Movement;
using Lumbre.Game.Domain.Combat;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class H10PlayerControlTests
    {
        [Test]
        public void LocomotionAcceleratesTowardCappedSpeed()
        {
            var model = new PlayerLocomotionModel(2.4f, 6f, 8f);

            var velocity = model.Tick(new MovementIntent(1f, 1f), 1f);

            Assert.That(velocity.Magnitude, Is.EqualTo(2.4f).Within(0.0001f));
        }

        [Test]
        public void LocomotionDeceleratesToRestWhenIntentIsReleased()
        {
            var model = new PlayerLocomotionModel(2.4f, 64f, 128f);
            model.Tick(new MovementIntent(1f, 0f), 0.1f);

            var velocity = model.Tick(new MovementIntent(0f, 0f), 0.1f);

            Assert.That(velocity.Magnitude, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void DiagonalIntentKeepsNormalizedMagnitude()
        {
            var intent = new MovementIntent(1f, 1f);

            Assert.That(intent.X * intent.X + intent.Y * intent.Y,
                Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ActionStateBlocksPausedAndDeadActions()
        {
            var model = new PlayerActionStateModel();

            Assert.IsTrue(model.TryBegin(PlayerActionState.Attacking));
            model.Complete(PlayerActionState.Attacking);
            model.SetPaused(true);
            Assert.IsFalse(model.TryBegin(PlayerActionState.Defending));
            model.SetPaused(false);
            model.SetDead(true);
            Assert.IsFalse(model.TryBegin(PlayerActionState.AreaAttack));
        }

        [Test]
        public void AttackSequenceMovesThroughAnticipationImpactAndRecoveryOnce()
        {
            var sequence = new AttackSequenceModel(0.1f, 0.2f);

            Assert.IsTrue(sequence.TryBegin(0f));
            Assert.AreEqual(AttackSequencePhase.Anticipation, sequence.Phase);
            Assert.IsFalse(sequence.TryEnterImpact(0.09f));
            Assert.IsTrue(sequence.TryEnterImpact(0.1f));
            Assert.IsTrue(sequence.TryConsumeImpact());
            Assert.IsFalse(sequence.TryConsumeImpact());
            Assert.IsTrue(sequence.TryEnterRecovery(0.1f));
            Assert.IsFalse(sequence.TryComplete(0.29f));
            Assert.IsTrue(sequence.TryComplete(0.3f));
            Assert.AreEqual(AttackSequencePhase.Idle, sequence.Phase);
        }

        [Test]
        public void AnalogResponsePreservesDeadZoneAndAllowsPartialSpeed()
        {
            var stopped = new MovementIntent(0.05f, 0f, 0.12f, 1f);
            var partial = new MovementIntent(0.56f, 0f, 0.12f, 1f);

            Assert.IsFalse(stopped.IsMoving);
            Assert.That(partial.X, Is.GreaterThan(0f).And.LessThan(1f));
        }

        [Test]
        public void AreaAttackExposesDomainReadinessWithoutChangingRadius()
        {
            var heat = new HeatResourceModel(100, 50);
            var time = new ManualCombatTimeSource();
            var area = new AreaAttackAbilityModel(time, heat, 50, 45, 2.2f, 2f);

            Assert.IsTrue(area.CanActivate);
            Assert.IsTrue(area.TryActivate().Succeeded);
            Assert.That(area.Radius, Is.EqualTo(2.2f).Within(0.0001f));
            Assert.IsFalse(area.CanActivate);
        }

        private sealed class ManualCombatTimeSource : ICombatTimeSource
        {
            public float Now { get; set; }
        }
    }
}
