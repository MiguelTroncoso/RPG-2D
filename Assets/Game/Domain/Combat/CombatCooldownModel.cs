using System;

namespace Lumbre.Game.Domain.Combat
{
    public sealed class CombatCooldownModel
    {
        private readonly ICombatTimeSource _timeSource;
        private readonly float _duration;
        private float _readyAt = float.NegativeInfinity;

        public CombatCooldownModel(ICombatTimeSource timeSource, float duration)
        {
            _timeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
            _duration = Math.Max(0f, duration);
        }

        public bool IsReady => _timeSource.Now >= _readyAt;
        public float Remaining => Math.Max(0f, _readyAt - _timeSource.Now);

        public bool TryStart()
        {
            if (!IsReady)
            {
                return false;
            }

            _readyAt = _timeSource.Now + _duration;
            return true;
        }
    }
}
