using System;
using Lumbre.Game.Domain.Combat;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class H4BasicAttacker : MonoBehaviour, IAttacker
    {
        [SerializeField, Min(1)] private int damage = 20;
        [SerializeField, Min(0f)] private float cooldownSeconds = 0.6f;
        [SerializeField] private string sourceId = "basic-attack";

        private BasicAttackerModel _model;

        public event Action<AttackResult> AttackPerformed;

        public int Damage => _model?.Damage ?? Mathf.Max(1, damage);
        public float CooldownSeconds => _model?.CooldownSeconds ?? Mathf.Max(0f, cooldownSeconds);

        private void Awake()
        {
            InitializeModel();
        }

        public void Configure(int attackDamage, float cooldown, string attackSourceId)
        {
            damage = Mathf.Max(1, attackDamage);
            cooldownSeconds = Mathf.Max(0f, cooldown);
            sourceId = string.IsNullOrWhiteSpace(attackSourceId)
                ? "basic-attack"
                : attackSourceId;
            if (UnityEngine.Application.isPlaying)
            {
                InitializeModel();
            }
        }

        public bool CanAttack(float time)
        {
            InitializeModelIfNeeded();
            return _model.CanAttack(time);
        }

        public AttackResult TryAttack(ITargetable target, float time)
        {
            InitializeModelIfNeeded();
            var result = _model.TryAttack(target, time);
            if (result.Succeeded)
            {
                AttackPerformed?.Invoke(result);
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
            _model = new BasicAttackerModel(damage, cooldownSeconds, sourceId);
        }
    }
}
