using System;

namespace Lumbre.Game.Domain.Combat
{
    public sealed class DefenseAbilityModel : IDamageModifier
    {
        private readonly ICombatTimeSource _timeSource;
        private readonly CombatCooldownModel _cooldown;
        private readonly float _duration;
        private readonly float _damageMultiplier;
        private float _activeUntil = float.NegativeInfinity;

        public DefenseAbilityModel(ICombatTimeSource timeSource, float duration,
            float cooldownSeconds, float damageReduction)
        {
            _timeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            _cooldown = new CombatCooldownModel(_timeSource, cooldownSeconds);
            _duration = Math.Max(0f, duration);
            var reduction = Math.Max(0f, Math.Min(0.95f, damageReduction));
            _damageMultiplier = 1f - reduction;
        }

        public bool IsActive => _timeSource.Now < _activeUntil;
        public float RemainingDuration => Math.Max(0f, _activeUntil - _timeSource.Now);
        public float CooldownRemaining => _cooldown.Remaining;
        public float DamageMultiplier => IsActive ? _damageMultiplier : 1f;

        public AbilityResult TryActivate()
        {
            if (IsActive)
            {
                return AbilityResult.Failure(AbilityResultCode.AlreadyActive);
            }

            if (!_cooldown.TryStart())
            {
                return AbilityResult.Failure(AbilityResultCode.Cooldown);
            }

            _activeUntil = _timeSource.Now + _duration;
            return AbilityResult.Success();
        }

        public CombatDamage Modify(CombatDamage damage)
        {
            if (!IsActive || damage.Amount <= 0)
            {
                return damage;
            }

            var reducedAmount = Math.Max(1,
                (int)Math.Round(damage.Amount * _damageMultiplier,
                    MidpointRounding.AwayFromZero));
            return new CombatDamage(reducedAmount, damage.SourceId);
        }
    }
}
