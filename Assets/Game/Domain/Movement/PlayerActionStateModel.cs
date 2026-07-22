namespace Lumbre.Game.Domain.Movement
{
    /// <summary>
    /// Small action gate shared by the client adapters. Cooldowns and combat
    /// rules remain in their existing domain models; this only prevents
    /// incompatible input from being accepted at the same instant.
    /// </summary>
    public sealed class PlayerActionStateModel
    {
        public PlayerActionState Current { get; private set; } = PlayerActionState.Idle;

        public bool IsDead => Current == PlayerActionState.Dead;
        public bool IsPaused => Current == PlayerActionState.Paused;

        public bool TryBegin(PlayerActionState action)
        {
            if (action == PlayerActionState.Dead || action == PlayerActionState.Paused
                || IsDead || IsPaused)
            {
                return false;
            }

            if (Current != PlayerActionState.Idle && Current != PlayerActionState.Moving)
            {
                return false;
            }

            Current = action;
            return true;
        }

        public void Complete(PlayerActionState action)
        {
            if (Current == action)
            {
                Current = PlayerActionState.Idle;
            }
        }

        public void SetMoving(bool moving)
        {
            if (Current != PlayerActionState.Idle && Current != PlayerActionState.Moving)
            {
                return;
            }

            Current = moving ? PlayerActionState.Moving : PlayerActionState.Idle;
        }

        public void SetPaused(bool paused)
        {
            if (IsDead)
            {
                return;
            }

            if (paused)
            {
                Current = PlayerActionState.Paused;
                return;
            }

            if (IsPaused)
            {
                Current = PlayerActionState.Idle;
            }
        }

        public void SetDead(bool dead)
        {
            if (dead)
            {
                Current = PlayerActionState.Dead;
                return;
            }

            if (IsDead)
            {
                Current = PlayerActionState.Idle;
            }
        }
    }
}
