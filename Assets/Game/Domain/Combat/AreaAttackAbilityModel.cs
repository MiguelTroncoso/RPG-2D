using System;

namespace Lumbre.Game.Domain.Combat
{
    public sealed class AreaAttackAbilityModel
    {
        private readonly HeatResourceModel _heat;
        private readonly CombatCooldownModel _cooldown;
        private readonly int _heatCost;
        private readonly int _damage;
        private readonly float _radius;

        public AreaAttackAbilityModel(ICombatTimeSource timeSource, HeatResourceModel heat,
            int heatCost, int damage, float radius, float cooldownSeconds)
        {
            _heat = heat ?? throw new ArgumentNullException(nameof(heat));
            _cooldown = new CombatCooldownModel(timeSource, cooldownSeconds);
            _heatCost = Math.Max(0, heatCost);
            _damage = Math.Max(1, damage);
            _radius = Math.Max(0.1f, radius);
        }

        public int HeatCost => _heatCost;
        public int Damage => _damage;
        public float Radius => _radius;
        public float CooldownRemaining => _cooldown.Remaining;

        public AbilityResult TryActivate()
        {
            if (!_cooldown.IsReady)
            {
                return AbilityResult.Failure(AbilityResultCode.Cooldown);
            }

            if (!_heat.TrySpend(_heatCost))
            {
                return AbilityResult.Failure(AbilityResultCode.InsufficientHeat);
            }

            _cooldown.TryStart();
            return AbilityResult.Success();
        }
    }
}
