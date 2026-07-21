using System;
using Lumbre.Game.Domain.Combat;
using Lumbre.Game.Client.Player;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class H4PlayerCombatController : MonoBehaviour
    {
        [SerializeField] private H3PlayerInputReader inputReader;
        [SerializeField] private H4BasicAttacker attacker;
        [SerializeField] private H4CombatHealth health;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField, Min(0.1f)] private float attackRange = 1.45f;

        public AttackResult LastAttackResult { get; private set; }
        public float AttackRange => attackRange;
        public event Action<AttackResult> BasicAttackSucceeded;

        public void Configure(H3PlayerInputReader reader, H4BasicAttacker basicAttacker,
            H4CombatHealth combatHealth, LayerMask layers, float range)
        {
            inputReader = reader;
            attacker = basicAttacker;
            health = combatHealth;
            targetLayers = layers;
            attackRange = Mathf.Max(0.1f, range);
        }

        private void Awake()
        {
            inputReader ??= GetComponent<H3PlayerInputReader>();
            attacker ??= GetComponent<H4BasicAttacker>();
            health ??= GetComponent<H4CombatHealth>();
        }

        private void Update()
        {
            if (inputReader != null && inputReader.AttackPressedThisFrame)
            {
                TryBasicAttack();
            }
        }

        public AttackResult TryBasicAttack()
        {
            if (health != null && !health.IsAlive)
            {
                LastAttackResult = AttackResult.Failure(AttackResultCode.InvalidTarget);
                return LastAttackResult;
            }

            var target = FindNearestTarget();
            if (target == null)
            {
                LastAttackResult = AttackResult.Failure(AttackResultCode.InvalidTarget);
                return LastAttackResult;
            }

            LastAttackResult = attacker.TryAttack(target, Time.time);
            if (LastAttackResult.Succeeded)
            {
                GetComponent<H4CombatFeedback>()?.PlayAttack();
                BasicAttackSucceeded?.Invoke(LastAttackResult);
            }

            return LastAttackResult;
        }

        private ITargetable FindNearestTarget()
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, attackRange, targetLayers);
            ITargetable nearest = null;
            var nearestDistance = float.PositiveInfinity;
            foreach (var collider in colliders)
            {
                var target = collider.GetComponentInParent<H4CombatHealth>();
                if (target == null || !target.IsTargetable)
                {
                    continue;
                }

                var distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = target;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
