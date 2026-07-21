namespace Lumbre.Game.Domain.Combat
{
    public sealed class MordeluzAiStateMachine
    {
        public MordeluzAiStateMachine()
        {
            Current = MordeluzAiState.Idle;
        }

        public MordeluzAiState Current { get; private set; }

        public MordeluzAiTransition Tick(MordeluzAiContext context)
        {
            var previous = Current;
            switch (Current)
            {
                case MordeluzAiState.Idle:
                    if (context.HasTarget && context.TargetAlive
                        && context.DistanceToTarget <= context.DetectionRange)
                    {
                        Current = MordeluzAiState.Detect;
                    }
                    break;

                case MordeluzAiState.Detect:
                    if (!context.HasTarget || !context.TargetAlive)
                    {
                        Current = MordeluzAiState.Return;
                    }
                    else if (context.DistanceToTarget <= context.AttackRange)
                    {
                        Current = MordeluzAiState.Attack;
                    }
                    else
                    {
                        Current = MordeluzAiState.Follow;
                    }
                    break;

                case MordeluzAiState.Follow:
                    if (!context.HasTarget || !context.TargetAlive
                        || context.DistanceFromSpawn > context.LeashRange)
                    {
                        Current = MordeluzAiState.Return;
                    }
                    else if (context.DistanceToTarget <= context.AttackRange)
                    {
                        Current = MordeluzAiState.Attack;
                    }
                    break;

                case MordeluzAiState.Attack:
                    if (!context.HasTarget || !context.TargetAlive
                        || context.DistanceFromSpawn > context.LeashRange)
                    {
                        Current = MordeluzAiState.Return;
                    }
                    else if (context.DistanceToTarget > context.AttackRange)
                    {
                        Current = MordeluzAiState.Follow;
                    }
                    break;

                case MordeluzAiState.Return:
                    if (context.DistanceFromSpawn <= 0.15f)
                    {
                        Current = MordeluzAiState.Idle;
                    }
                    break;

                case MordeluzAiState.Dead:
                    break;
            }

            return new MordeluzAiTransition(previous, Current);
        }

        public void SetDead()
        {
            Current = MordeluzAiState.Dead;
        }
    }
}
