using Lumbre.Game.Domain.Combat;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class CombatPrototypeTests
    {
        [Test]
        public void HealthAppliesDamageAndClampsAtZero()
        {
            var health = new CombatHealthModel(50);
            var result = health.ReceiveDamage(new CombatDamage(65, "test"));

            Assert.IsTrue(result.Applied);
            Assert.IsTrue(result.Killed);
            Assert.AreEqual(50, result.PreviousHealth);
            Assert.AreEqual(0, result.CurrentHealth);
            Assert.IsFalse(health.IsAlive);

            var afterDeath = health.ReceiveDamage(new CombatDamage(10, "test"));
            Assert.IsFalse(afterDeath.Applied);
            Assert.AreEqual(0, health.CurrentHealth);
        }

        [Test]
        public void BasicAttackerHonorsCooldownAndTargetHealth()
        {
            var attacker = new BasicAttackerModel(10, 1f, "test-attack");
            var target = new TestTarget(30);

            var first = attacker.TryAttack(target, 0f);
            var healthAfterFirst = target.Health.CurrentHealth;
            var blocked = attacker.TryAttack(target, 0.5f);
            var healthDuringCooldown = target.Health.CurrentHealth;
            var second = attacker.TryAttack(target, 1f);

            Assert.IsTrue(first.Succeeded);
            Assert.AreEqual(20, healthAfterFirst);
            Assert.AreEqual(AttackResultCode.Cooldown, blocked.Code);
            Assert.AreEqual(20, healthDuringCooldown);
            Assert.IsTrue(second.Succeeded);
            Assert.AreEqual(10, target.Health.CurrentHealth);
        }

        [Test]
        public void MordeluzTransitionsThroughCombatLoopAndReturns()
        {
            var machine = new MordeluzAiStateMachine();
            var detecting = new MordeluzAiContext(true, true, 3f, 0f, 4.5f, 1f, 6.5f);
            var attacking = new MordeluzAiContext(true, true, 0.8f, 0f, 4.5f, 1f, 6.5f);
            var targetLost = new MordeluzAiContext(true, false, 0.8f, 2f, 4.5f, 1f, 6.5f);
            var atSpawn = new MordeluzAiContext(false, false, 99f, 0f, 4.5f, 1f, 6.5f);

            Assert.AreEqual(MordeluzAiState.Detect, machine.Tick(detecting).Current);
            Assert.AreEqual(MordeluzAiState.Follow, machine.Tick(detecting).Current);
            Assert.AreEqual(MordeluzAiState.Attack, machine.Tick(attacking).Current);
            Assert.AreEqual(MordeluzAiState.Return, machine.Tick(targetLost).Current);
            Assert.AreEqual(MordeluzAiState.Idle, machine.Tick(atSpawn).Current);
        }

        private sealed class TestTarget : ITargetable
        {
            public TestTarget(int maxHealth)
            {
                Model = new CombatHealthModel(maxHealth);
            }

            public CombatHealthModel Model { get; }
            public bool IsTargetable => Model.IsAlive;
            public IHealth Health => Model;
            public IDamageable Damageable => Model;
        }
    }
}
