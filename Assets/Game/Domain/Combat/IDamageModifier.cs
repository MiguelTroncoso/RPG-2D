namespace Lumbre.Game.Domain.Combat
{
    public interface IDamageModifier
    {
        CombatDamage Modify(CombatDamage damage);
    }
}
