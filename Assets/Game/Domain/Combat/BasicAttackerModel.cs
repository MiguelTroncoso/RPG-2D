using System;

namespace Lumbre.Game.Domain.Combat
{
    public sealed class BasicAttackerModel : IAttacker
    {
        private readonly int _damage;
        private readonly float _cooldownSeconds;
        private readonly string _sourceId;
        private float _lastAttackTime = float.NegativeInfinity;

        public BasicAttackerModel(int damage, float cooldownSeconds, string sourceId)
        {
            _damage = Math.Max(1, damage);
            _cooldownSeconds = Math.Max(0f, cooldownSeconds);
            _sourceId = sourceId ?? "basic-attack";
        }

        public int Damage => _damage;
        public float CooldownSeconds => _cooldownSeconds;

        public bool CanAttack(float time)
        {
            return time >= _lastAttackTime + _cooldownSeconds;
        }

        public AttackResult TryAttack(ITargetable target, float time)
        {
            if (target == null || !target.IsTargetable || target.Damageable == null)
            {
                return AttackResult.Failure(AttackResultCode.InvalidTarget);
            }

            if (!target.Health.IsAlive)
            {
                return AttackResult.Failure(AttackResultCode.TargetDead);
            }

            if (!CanAttack(time))
            {
                return AttackResult.Failure(AttackResultCode.Cooldown);
            }

            _lastAttackTime = time;
            var damage = target.Damageable.ReceiveDamage(new CombatDamage(_damage, _sourceId));
            return AttackResult.Success(damage);
        }
    }
}
