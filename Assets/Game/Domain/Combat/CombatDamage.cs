using System;

namespace Lumbre.Game.Domain.Combat
{
    public readonly struct CombatDamage : IEquatable<CombatDamage>
    {
        public CombatDamage(int amount, string sourceId)
        {
            Amount = Math.Max(0, amount);
            SourceId = sourceId ?? string.Empty;
        }

        public int Amount { get; }
        public string SourceId { get; }

        public bool Equals(CombatDamage other)
        {
            return Amount == other.Amount && string.Equals(SourceId, other.SourceId,
                StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is CombatDamage other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Amount * 397) ^ SourceId.GetHashCode();
            }
        }
    }
}
