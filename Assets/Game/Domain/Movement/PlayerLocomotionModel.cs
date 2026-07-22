using System;

namespace Lumbre.Game.Domain.Movement
{
    /// <summary>
    /// Frame-rate independent acceleration and deceleration for player intent.
    /// This model deliberately has no Unity dependency so it can be reused by
    /// an offline client and by a future authoritative simulation.
    /// </summary>
    public sealed class PlayerLocomotionModel
    {
        private readonly float _maxSpeed;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private LocomotionVelocity _velocity;

        public PlayerLocomotionModel(float maxSpeed, float acceleration, float deceleration)
        {
            _maxSpeed = Math.Max(0f, maxSpeed);
            _acceleration = Math.Max(0f, acceleration);
            _deceleration = Math.Max(0f, deceleration);
        }

        public LocomotionVelocity Velocity => _velocity;

        public LocomotionVelocity Tick(MovementIntent intent, float deltaTime)
        {
            var safeDeltaTime = Math.Max(0f, deltaTime);
            var targetX = intent.X * _maxSpeed;
            var targetY = intent.Y * _maxSpeed;
            var targetSqrMagnitude = (targetX * targetX) + (targetY * targetY);
            var currentSqrMagnitude = _velocity.SqrMagnitude;
            var accelerating = targetSqrMagnitude > currentSqrMagnitude + 0.000001f;
            var rate = accelerating ? _acceleration : _deceleration;
            var step = rate * safeDeltaTime;

            var nextX = MoveTowards(_velocity.X, targetX, step);
            var nextY = MoveTowards(_velocity.Y, targetY, step);
            var nextMagnitude = (float)Math.Sqrt((nextX * nextX) + (nextY * nextY));
            if (nextMagnitude > _maxSpeed && nextMagnitude > 0.0001f)
            {
                var scale = _maxSpeed / nextMagnitude;
                nextX *= scale;
                nextY *= scale;
            }

            _velocity = new LocomotionVelocity(nextX, nextY);
            return _velocity;
        }

        public void Reset()
        {
            _velocity = new LocomotionVelocity(0f, 0f);
        }

        private static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + Math.Sign(target - current) * maxDelta;
        }
    }
}
