using System;

namespace Lumbre.Game.Domain.Movement
{
    public readonly struct MovementIntent : IEquatable<MovementIntent>
    {
        public MovementIntent(float x, float y)
            : this(x, y, 0.1f, 1f)
        {
        }

        public MovementIntent(float x, float y, float deadZone, float responseExponent)
        {
            var magnitude = Math.Sqrt((x * x) + (y * y));
            var threshold = Math.Max(0d, Math.Min(0.99d, deadZone));
            if (magnitude <= threshold)
            {
                X = 0f;
                Y = 0f;
                return;
            }

            var normalizedX = x / magnitude;
            var normalizedY = y / magnitude;
            var remappedMagnitude = Math.Min(1d,
                Math.Max(0d, (magnitude - threshold) / Math.Max(0.0001d, 1d - threshold)));
            var exponent = Math.Max(0.01d, responseExponent);
            remappedMagnitude = Math.Pow(remappedMagnitude, exponent);

            X = (float)(normalizedX * remappedMagnitude);
            Y = (float)(normalizedY * remappedMagnitude);
        }

        public float X { get; }
        public float Y { get; }
        public bool IsMoving => (X * X) + (Y * Y) > 0f;

        public bool Equals(MovementIntent other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is MovementIntent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static bool operator ==(MovementIntent left, MovementIntent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementIntent left, MovementIntent right)
        {
            return !left.Equals(right);
        }
    }
}
