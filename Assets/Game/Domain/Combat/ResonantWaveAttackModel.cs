using System;

namespace Lumbre.Game.Domain.Combat
{
    public sealed class ResonantWaveAttackModel
    {
        private readonly ICombatTimeSource _timeSource;
        private readonly CombatCooldownModel _cooldown;
        private readonly float _windupSeconds;
        private float _resolveAt = float.PositiveInfinity;
        private bool _pending;

        public ResonantWaveAttackModel(ICombatTimeSource timeSource, float windupSeconds,
            float cooldownSeconds)
        {
            _timeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            _cooldown = new CombatCooldownModel(_timeSource, cooldownSeconds);
            _windupSeconds = Math.Max(0f, windupSeconds);
        }

        public bool IsTelegraphActive => _pending;
        public bool CanResolve => _pending && _timeSource.Now >= _resolveAt;
        public float RemainingWindup => _pending
            ? Math.Max(0f, _resolveAt - _timeSource.Now)
            : 0f;
        public float CooldownRemaining => _cooldown.Remaining;

        public AbilityResult TryBegin()
        {
            if (_pending)
            {
                return AbilityResult.Failure(AbilityResultCode.AlreadyActive);
            }

            if (!_cooldown.TryStart())
            {
                return AbilityResult.Failure(AbilityResultCode.Cooldown);
            }

            _pending = true;
            _resolveAt = _timeSource.Now + _windupSeconds;
            return AbilityResult.Success();
        }

        public AbilityResult TryResolve()
        {
            if (!CanResolve)
            {
                return AbilityResult.Failure(AbilityResultCode.NotReady);
            }

            _pending = false;
            _resolveAt = float.PositiveInfinity;
            return AbilityResult.Success();
        }
    }
}
