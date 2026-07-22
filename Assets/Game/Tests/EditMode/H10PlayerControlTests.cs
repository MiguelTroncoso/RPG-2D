using Lumbre.Game.Domain.Movement;
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
    }
}
