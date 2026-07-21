using System;

namespace Lumbre.Game.Domain.Combat
{
    public sealed class HeatResourceModel
    {
        private readonly int _maxHeat;
        private int _currentHeat;

        public HeatResourceModel(int maxHeat, int initialHeat = 0)
        {
            _maxHeat = Math.Max(1, maxHeat);
            _currentHeat = Clamp(initialHeat);
        }

        public int CurrentHeat => _currentHeat;
        public int MaxHeat => _maxHeat;

        public bool CanSpend(int amount)
        {
            return amount >= 0 && _currentHeat >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            _currentHeat -= amount;
            return true;
        }

        public int Add(int amount)
        {
            _currentHeat = Clamp(_currentHeat + Math.Max(0, amount));
            return _currentHeat;
        }

        public void Reset()
        {
            _currentHeat = 0;
        }

        private int Clamp(int value)
        {
            return Math.Max(0, Math.Min(_maxHeat, value));
        }
    }
}
