namespace Lumbre.Game.Domain.Combat
{
    public readonly struct DamageResult
    {
        public DamageResult(bool applied, int amount, int previousHealth, int currentHealth,
            bool killed)
        {
            Applied = applied;
            Amount = amount;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            Killed = killed;
        }

        public bool Applied { get; }
        public int Amount { get; }
        public int PreviousHealth { get; }
        public int CurrentHealth { get; }
        public bool Killed { get; }
    }
}
