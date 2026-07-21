using System;

namespace Lumbre.Game.Domain.Movement
{
    public readonly struct MovementIntent : IEquatable<MovementIntent>
    {
        public MovementIntent(float x, float y)
        {
            var magnitude = Math.Sqrt((x * x) + (y * y));
            if (magnitude < 0.1d)
            {
                X = 0f;
                Y = 0f;
                return;
            }

            if (magnitude > 1d)
            {
                x = (float)(x / magnitude);
                y = (float)(y / magnitude);
            }

            X = x;
            Y = y;
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
