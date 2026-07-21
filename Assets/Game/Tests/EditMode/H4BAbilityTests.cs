using Lumbre.Game.Domain.Combat;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class H4BAbilityTests
    {
        [Test]
        public void HeatAddsSpendsAndClampsToConfiguredMaximum()
        {
            var heat = new HeatResourceModel(100, 10);

            Assert.AreEqual(60, heat.Add(50));
            Assert.IsTrue(heat.TrySpend(40));
            Assert.AreEqual(20, heat.CurrentHeat);
            Assert.IsFalse(heat.TrySpend(21));
            Assert.AreEqual(20, heat.CurrentHeat);
            Assert.AreEqual(100, heat.Add(200));
        }

        [Test]
        public void DefenseReducesDamageOnlyDuringDurationAndHonorsCooldown()
        {
            var time = new ManualTimeSource();
            var defense = new DefenseAbilityModel(time, 1f, 3f, 0.65f);

            Assert.IsTrue(defense.TryActivate().Succeeded);
            Assert.IsTrue(defense.IsActive);
            Assert.AreEqual(4, defense.Modify(new CombatDamage(10, "test")).Amount);

            time.Now = 1.01f;
            Assert.IsFalse(defense.IsActive);
            Assert.AreEqual(10, defense.Modify(new CombatDamage(10, "test")).Amount);
            Assert.AreEqual(AbilityResultCode.Cooldown, defense.TryActivate().Code);

            time.Now = 3.01f;
            Assert.IsTrue(defense.TryActivate().Succeeded);
        }

        [Test]
        public void AreaAttackRequiresHeatAndHonorsCooldown()
        {
            var time = new ManualTimeSource();
            var heat = new HeatResourceModel(100, 50);
            var area = new AreaAttackAbilityModel(time, heat, 50, 45, 2.2f, 2f);

            Assert.IsTrue(area.TryActivate().Succeeded);
            Assert.AreEqual(0, heat.CurrentHeat);
            Assert.AreEqual(AbilityResultCode.Cooldown, area.TryActivate().Code);

            time.Now = 2.01f;
            Assert.AreEqual(AbilityResultCode.InsufficientHeat, area.TryActivate().Code);
            heat.Add(50);
            Assert.IsTrue(area.TryActivate().Succeeded);
        }

        [Test]
        public void ResonantWaveHasTelegraphWindowAndCooldown()
        {
            var time = new ManualTimeSource();
            var wave = new ResonantWaveAttackModel(time, 0.8f, 2.4f);

            Assert.IsTrue(wave.TryBegin().Succeeded);
            Assert.IsTrue(wave.IsTelegraphActive);
            Assert.AreEqual(AbilityResultCode.NotReady, wave.TryResolve().Code);

            time.Now = 0.8f;
            Assert.IsTrue(wave.CanResolve);
            Assert.IsTrue(wave.TryResolve().Succeeded);
            Assert.IsFalse(wave.IsTelegraphActive);
            Assert.AreEqual(AbilityResultCode.Cooldown, wave.TryBegin().Code);

            time.Now = 2.41f;
            Assert.IsTrue(wave.TryBegin().Succeeded);
        }

        private sealed class ManualTimeSource : ICombatTimeSource
        {
            public float Now { get; set; }
        }
    }
}
