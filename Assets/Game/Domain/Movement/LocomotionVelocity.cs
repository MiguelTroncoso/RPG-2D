namespace Lumbre.Game.Domain.Movement
{
    public readonly struct LocomotionVelocity
    {
        public LocomotionVelocity(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
        public float SqrMagnitude => (X * X) + (Y * Y);
        public float Magnitude => (float)System.Math.Sqrt(SqrMagnitude);
        public bool IsMoving => SqrMagnitude > 0.0001f;
    }
}
