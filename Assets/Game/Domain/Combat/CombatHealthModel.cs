using System;

namespace Lumbre.Game.Domain.Combat
{
    public sealed class CombatHealthModel : IHealth, IDamageable
    {
        private readonly int _maxHealth;
        private int _currentHealth;

        public CombatHealthModel(int maxHealth)
        {
            _maxHealth = Math.Max(1, maxHealth);
            _currentHealth = _maxHealth;
        }

        public event Action<DamageResult> DamageReceived;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsAlive => _currentHealth > 0;

        public DamageResult ReceiveDamage(CombatDamage damage)
        {
            if (!IsAlive || damage.Amount <= 0)
            {
                return new DamageResult(false, 0, _currentHealth, _currentHealth, false);
            }

            var previousHealth = _currentHealth;
            _currentHealth = Math.Max(0, _currentHealth - damage.Amount);
            var result = new DamageResult(
                true,
                previousHealth - _currentHealth,
                previousHealth,
                _currentHealth,
                _currentHealth == 0);
            DamageReceived?.Invoke(result);
            return result;
        }

        public void RestoreToFull()
        {
            _currentHealth = _maxHealth;
        }
    }
}
