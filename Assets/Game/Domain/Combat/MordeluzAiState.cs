namespace Lumbre.Game.Domain.Combat
{
    public enum MordeluzAiState
    {
        Idle = 0,
        Detect = 1,
        Follow = 2,
        Attack = 3,
        Return = 4,
        Dead = 5
    }

    public readonly struct MordeluzAiContext
    {
        public MordeluzAiContext(bool hasTarget, bool targetAlive, float distanceToTarget,
            float distanceFromSpawn, float detectionRange, float attackRange, float leashRange)
        {
            HasTarget = hasTarget;
            TargetAlive = targetAlive;
            DistanceToTarget = distanceToTarget;
            DistanceFromSpawn = distanceFromSpawn;
            DetectionRange = detectionRange;
            AttackRange = attackRange;
            LeashRange = leashRange;
        }

        public bool HasTarget { get; }
        public bool TargetAlive { get; }
        public float DistanceToTarget { get; }
        public float DistanceFromSpawn { get; }
        public float DetectionRange { get; }
        public float AttackRange { get; }
        public float LeashRange { get; }
    }

    public readonly struct MordeluzAiTransition
    {
        public MordeluzAiTransition(MordeluzAiState previous, MordeluzAiState current)
        {
            Previous = previous;
            Current = current;
        }

        public MordeluzAiState Previous { get; }
        public MordeluzAiState Current { get; }
        public bool Changed => Previous != Current;
    }
}
