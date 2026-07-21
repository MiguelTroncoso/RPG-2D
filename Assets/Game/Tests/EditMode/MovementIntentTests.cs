using Lumbre.Game.Domain.Movement;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class MovementIntentTests
    {
        [Test]
        public void IntentClampsMagnitudeAndPreservesDirection()
        {
            var intent = new MovementIntent(3f, 4f);

            Assert.That(intent.X, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(intent.Y, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.IsTrue(intent.IsMoving);
        }

        [Test]
        public void IntentDeadZoneProducesNoMovement()
        {
            var intent = new MovementIntent(0.05f, 0.05f);

            Assert.AreEqual(0f, intent.X);
            Assert.AreEqual(0f, intent.Y);
            Assert.IsFalse(intent.IsMoving);
        }
    }
}
