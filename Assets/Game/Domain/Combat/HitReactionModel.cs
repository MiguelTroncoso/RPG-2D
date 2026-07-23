using System;

namespace Lumbre.Game.Domain.Combat
{
    /// <summary>
    /// Visible hit reaction for a combatant: a short stagger window that
    /// interrupts its actions plus an eased knockback displacement away from
    /// the damage source. Pure C#: the client applies the returned deltas to
    /// its physics body and asks <see cref="IsStaggered"/> before acting.
    /// </summary>
    public sealed class HitReactionModel
    {
        private readonly float _staggerSeconds;
        private readonly float _knockbackDistance;
        private readonly float _knockbackSeconds;

        private float _hitTime = float.NegativeInfinity;
        private float _directionX;
        private float _directionY;
        private float _appliedDistance;

        public HitReactionModel(float staggerSeconds, float knockbackDistance,
            float knockbackSeconds)
        {
            _staggerSeconds = Math.Max(0f, staggerSeconds);
            _knockbackDistance = Math.Max(0f, knockbackDistance);
            _knockbackSeconds = Math.Max(0.01f, knockbackSeconds);
        }

        public float StaggerSeconds => _staggerSeconds;

        public bool IsStaggered(float time)
        {
            return time >= _hitTime && time < _hitTime + _staggerSeconds;
        }

        /// <summary>Restarts the reaction pushing away along (directionX, directionY).</summary>
        public void ApplyHit(float time, float directionX, float directionY)
        {
            var magnitude = (float)Math.Sqrt(
                (directionX * directionX) + (directionY * directionY));
            if (magnitude < 0.0001f)
            {
                directionX = 1f;
                directionY = 0f;
                magnitude = 1f;
            }

            _hitTime = time;
            _directionX = directionX / magnitude;
            _directionY = directionY / magnitude;
            _appliedDistance = 0f;
        }

        /// <summary>
        /// Displacement to apply this tick. Eases out so the push starts
        /// strong and settles; total displacement never exceeds the
        /// configured knockback distance.
        /// </summary>
        public (float X, float Y) TickDisplacement(float time)
        {
            var elapsed = time - _hitTime;
            if (elapsed < 0f || _knockbackDistance <= 0f)
            {
                return (0f, 0f);
            }

            var progress = Math.Min(1f, elapsed / _knockbackSeconds);
            var eased = 1f - (1f - progress) * (1f - progress);
            var targetDistance = _knockbackDistance * eased;
            var step = targetDistance - _appliedDistance;
            if (step <= 0f)
            {
                return (0f, 0f);
            }

            _appliedDistance = targetDistance;
            return (_directionX * step, _directionY * step);
        }

        public void Reset()
        {
            _hitTime = float.NegativeInfinity;
            _appliedDistance = 0f;
        }
    }
}
