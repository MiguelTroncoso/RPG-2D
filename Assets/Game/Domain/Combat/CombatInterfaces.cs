namespace Lumbre.Game.Domain.Combat
{
    public interface IHealth
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsAlive { get; }
    }

    public interface IDamageable
    {
        DamageResult ReceiveDamage(CombatDamage damage);
    }

    public interface ITargetable
    {
        bool IsTargetable { get; }
        IHealth Health { get; }
        IDamageable Damageable { get; }
    }

    public interface IAttacker
    {
        int Damage { get; }
        float CooldownSeconds { get; }
        bool CanAttack(float time);
        AttackResult TryAttack(ITargetable target, float time);
    }
}
