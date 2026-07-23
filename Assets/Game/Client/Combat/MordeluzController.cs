using System;
using Lumbre.Game.Domain.Combat;
using Lumbre.Game.Domain.Constants;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public class MordeluzController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private H4CombatHealth health;
        [SerializeField] private H4BasicAttacker attacker;
        [SerializeField, Min(0.1f)] private float detectionRange = 4.5f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.05f;
        [SerializeField, Min(0.1f)] private float leashRange = 6.5f;
        [SerializeField, Min(0.1f)] private float movementSpeed = 1.2f;
        [SerializeField, Min(0f)] private float staggerSeconds =
            ProjectConstants.MordeluzStaggerSeconds;
        [SerializeField, Min(0f)] private float knockbackDistance =
            ProjectConstants.MordeluzKnockbackDistance;
        [SerializeField, Min(0.01f)] private float knockbackSeconds =
            ProjectConstants.MordeluzKnockbackSeconds;

        private Rigidbody2D _body;
        private MordeluzAiStateMachine _stateMachine;
        private Vector2 _spawnPosition;
        private HitReactionModel _hitReaction;

        public event Action<MordeluzAiTransition> StateChanged;

        public MordeluzAiState CurrentState => _stateMachine?.Current ?? MordeluzAiState.Idle;

        /// <summary>H10.3: true while the visible hit reaction interrupts the AI.</summary>
        public bool IsStaggered => _hitReaction != null && _hitReaction.IsStaggered(Time.time);
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public float LeashRange => leashRange;

        protected H4CombatHealth HealthComponent => health;
        protected H4BasicAttacker AttackerComponent => attacker;
        protected Transform TargetTransform => target;
        protected Vector2 SpawnPosition => _spawnPosition;
        protected virtual bool HasPendingAttack => false;
        protected virtual float StateAttackRange => HasPendingAttack
            ? float.PositiveInfinity
            : attackRange;

        public void Configure(Transform targetTransform, Transform returnPoint,
            H4CombatHealth combatHealth, H4BasicAttacker basicAttacker, float detection,
            float range, float leash, float speed)
        {
            target = targetTransform;
            spawnPoint = returnPoint;
            health = combatHealth;
            attacker = basicAttacker;
            detectionRange = Mathf.Max(0.1f, detection);
            attackRange = Mathf.Max(0.1f, range);
            leashRange = Mathf.Max(0.1f, leash);
            movementSpeed = Mathf.Max(0.1f, speed);
        }

        protected virtual void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            health ??= GetComponent<H4CombatHealth>();
            attacker ??= GetComponent<H4BasicAttacker>();
            target ??= GameObject.FindWithTag("Player")?.transform;
            _spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
            _stateMachine = new MordeluzAiStateMachine();
            _hitReaction = new HitReactionModel(staggerSeconds, knockbackDistance,
                knockbackSeconds);
            IgnoreTargetCollision();
            if (health != null)
            {
                health.Died += HandleDeath;
                health.DamageTaken += HandleDamageTaken;
            }
        }

        protected virtual void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
                health.DamageTaken -= HandleDamageTaken;
            }
        }

        protected virtual void Update()
        {
            if (health == null || !health.IsAlive)
            {
                _stateMachine.SetDead();
                return;
            }

            var targetHealth = target != null
                ? target.GetComponentInParent<H4CombatHealth>()
                : null;
            var targetAlive = targetHealth != null && targetHealth.IsTargetable;
            var distanceToTarget = target != null
                ? Vector2.Distance(transform.position, target.position)
                : float.PositiveInfinity;
            var distanceFromSpawn = Vector2.Distance(transform.position, _spawnPosition);
            var context = new MordeluzAiContext(
                target != null,
                targetAlive,
                distanceToTarget,
                distanceFromSpawn,
                detectionRange,
                StateAttackRange,
                leashRange);
            var transition = _stateMachine.Tick(context);
            if (transition.Changed)
            {
                StateChanged?.Invoke(transition);
            }

            if (_stateMachine.Current == MordeluzAiState.Attack && targetAlive
                && !IsStaggered
                && (distanceToTarget <= attackRange || HasPendingAttack))
            {
                PerformAttack(targetHealth, distanceToTarget);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (_body == null || _stateMachine == null)
            {
                return;
            }

            // H10.3 hit reaction: while staggered the enemy stops chasing and
            // is pushed away from the damage source so the hit reads clearly.
            if (IsStaggered)
            {
                var (knockX, knockY) = _hitReaction.TickDisplacement(Time.time);
                if (Mathf.Abs(knockX) > 0f || Mathf.Abs(knockY) > 0f)
                {
                    _body.MovePosition(_body.position + new Vector2(knockX, knockY));
                }

                return;
            }

            if (_stateMachine.Current == MordeluzAiState.Follow && target != null)
            {
                MoveTowards(target.position);
            }
            else if (_stateMachine.Current == MordeluzAiState.Return)
            {
                MoveTowards(_spawnPosition);
            }
        }

        private void MoveTowards(Vector3 destination)
        {
            var direction = (Vector2)destination - _body.position;
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            _body.MovePosition(_body.position
                + direction.normalized * movementSpeed * Time.fixedDeltaTime);
        }

        protected virtual void PerformAttack(H4CombatHealth targetHealth, float distanceToTarget)
        {
            var result = attacker.TryAttack(targetHealth, Time.time);
            if (result.Succeeded)
            {
                GetComponent<H4CombatFeedback>()?.PlayAttack();
            }
        }

        private void HandleDamageTaken(DamageResult result)
        {
            if (_hitReaction == null || !result.Applied || result.Killed)
            {
                return;
            }

            var away = target != null
                ? (Vector2)transform.position - (Vector2)target.position
                : Vector2.right;
            _hitReaction.ApplyHit(Time.time, away.x, away.y);
        }

        private void HandleDeath()
        {
            _stateMachine?.SetDead();
            _hitReaction?.Reset();
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
            }
        }

        private void IgnoreTargetCollision()
        {
            var ownCollider = GetComponent<Collider2D>();
            var targetCollider = target != null
                ? target.GetComponentInParent<Collider2D>()
                : null;
            if (ownCollider != null && targetCollider != null)
            {
                Physics2D.IgnoreCollision(ownCollider, targetCollider, true);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.65f, 0.1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
            if (spawnPoint != null)
            {
                Gizmos.color = new Color(0.2f, 1f, 0.85f, 0.8f);
                Gizmos.DrawLine(transform.position, spawnPoint.position);
                Gizmos.DrawWireSphere(spawnPoint.position, leashRange);
            }
        }
    }
}
