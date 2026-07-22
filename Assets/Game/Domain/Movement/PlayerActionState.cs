namespace Lumbre.Game.Domain.Movement
{
    public enum PlayerActionState
    {
        Idle = 0,
        Moving = 1,
        Attacking = 2,
        Defending = 3,
        AreaAttack = 4,
        Hurt = 5,
        Dead = 6,
        Paused = 7,
        Interacting = 8
    }
}
