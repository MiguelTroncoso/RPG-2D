using System;
using Lumbre.Game.Domain.Combat;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class H4CombatHealth : MonoBehaviour, IHealth, IDamageable, ITargetable
    {
        [SerializeField, Min(1)] private int maxHealth = 100;

        private CombatHealthModel _model;

        public event Action<DamageResult> DamageTaken;
        public event Action Died;

        public int CurrentHealth => _model?.CurrentHealth ?? maxHealth;
        public int MaxHealth => _model?.MaxHealth ?? Mathf.Max(1, maxHealth);
        public bool IsAlive => _model?.IsAlive ?? true;
        public bool IsTargetable => IsAlive && isActiveAndEnabled;
        public IHealth Health => this;
        public IDamageable Damageable => this;
        public IDamageModifier DamageModifier { get; set; }

        private void Awake()
        {
            InitializeModel();
        }

        public void ConfigureMaxHealth(int value)
        {
            maxHealth = Mathf.Max(1, value);
            if (UnityEngine.Application.isPlaying)
            {
                InitializeModel();
            }
        }

        public void RestoreToFull()
        {
            InitializeModel();
        }

        public DamageResult ReceiveDamage(CombatDamage damage)
        {
            InitializeModelIfNeeded();
            var modifiedDamage = DamageModifier?.Modify(damage) ?? damage;
            var result = _model.ReceiveDamage(modifiedDamage);
            if (!result.Applied)
            {
                return result;
            }

            DamageTaken?.Invoke(result);
            if (result.Killed)
            {
                Died?.Invoke();
            }

            return result;
        }

        private void InitializeModelIfNeeded()
        {
            if (_model == null)
            {
                InitializeModel();
            }
        }

        private void InitializeModel()
        {
            _model = new CombatHealthModel(Mathf.Max(1, maxHealth));
        }
    }
}
