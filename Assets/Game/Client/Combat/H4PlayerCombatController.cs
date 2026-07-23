using System;
using Lumbre.Game.Domain.Combat;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Movement;
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

        private H10PlayerActionStateController actionState;
        private H4CombatHealth recentAttackTarget;
        private float recentAttackTargetAt = float.NegativeInfinity;
        private const float RecentTargetContinuitySeconds = 0.75f;
        private AttackSequenceModel _sequence;

        public AttackResult LastAttackResult { get; private set; }
        public float AttackRange => attackRange;

        /// <summary>
        /// Raised when a basic attack lands. For input-driven attacks this
        /// fires at the visual impact of the H10.3 sequence, never at button
        /// press, so effects and damage stay on the same frame.
        /// </summary>
        public event Action<AttackResult> BasicAttackSucceeded;
        public event Action<AttackPhase> AttackPhaseChanged;

        public AttackPhase CurrentAttackPhase => _sequence?.Phase ?? AttackPhase.Idle;
        public float CurrentAttackPhaseProgress => _sequence?.PhaseProgress(Time.time) ?? 0f;
        public bool IsAttackSequenceActive => _sequence != null && _sequence.IsActive;
        public float AttackImpactDelaySeconds => _sequence?.ImpactDelaySeconds
            ?? ProjectConstants.PlayerAttackWindupSeconds
            + ProjectConstants.PlayerAttackStrikeSeconds;
        public float AttackSequenceSeconds => _sequence?.TotalSeconds
            ?? ProjectConstants.PlayerAttackWindupSeconds
            + ProjectConstants.PlayerAttackStrikeSeconds
            + ProjectConstants.PlayerAttackImpactSeconds
            + ProjectConstants.PlayerAttackRecoverySeconds;

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
            actionState ??= GetComponent<H10PlayerActionStateController>();
            _sequence = new AttackSequenceModel(
                ProjectConstants.PlayerAttackWindupSeconds,
                ProjectConstants.PlayerAttackStrikeSeconds,
                ProjectConstants.PlayerAttackImpactSeconds,
                ProjectConstants.PlayerAttackRecoverySeconds);
        }

        private void Update()
        {
            TickSequence();
            if (inputReader != null && inputReader.AttackPressedThisFrame)
            {
                TryBeginAttackSequence();
            }
        }

        private void OnDisable()
        {
            CancelSequence();
        }

        /// <summary>
        /// Synchronous programmatic attack kept for QA flows and regression
        /// tests (H4B/H5/H6). Gameplay input goes through the visible H10.3
        /// sequence instead.
        /// </summary>
        public AttackResult TryBasicAttack()
        {
            return ResolveAttack();
        }

        private void TryBeginAttackSequence()
        {
            if (_sequence == null || _sequence.IsActive)
            {
                return;
            }

            if (Time.timeScale <= 0f || health != null && !health.IsAlive)
            {
                LastAttackResult = AttackResult.Failure(AttackResultCode.InvalidTarget);
                return;
            }

            // The cooldown keeps measuring impact-to-impact, so the H4 damage
            // cadence is unchanged: a press is accepted when its future
            // impact instant clears the attacker cooldown.
            if (attacker != null
                && !attacker.CanAttack(Time.time + _sequence.ImpactDelaySeconds))
            {
                LastAttackResult = AttackResult.Failure(AttackResultCode.Cooldown);
                return;
            }

            if (actionState != null && !actionState.TryBegin(PlayerActionState.Attacking))
            {
                LastAttackResult = AttackResult.Failure(AttackResultCode.InvalidTarget);
                return;
            }

            _sequence.TryStart(Time.time);
            AttackPhaseChanged?.Invoke(AttackPhase.Windup);
        }

        private void TickSequence()
        {
            if (_sequence == null || !_sequence.IsActive)
            {
                return;
            }

            if (Time.timeScale <= 0f || health != null && !health.IsAlive)
            {
                CancelSequence();
                return;
            }

            var tick = _sequence.Tick(Time.time);
            if (tick.ImpactStarted)
            {
                ResolveAttack();
            }

            if (tick.PhaseChanged)
            {
                AttackPhaseChanged?.Invoke(tick.Phase);
            }

            if (tick.Completed)
            {
                actionState?.Complete(PlayerActionState.Attacking);
                AttackPhaseChanged?.Invoke(AttackPhase.Idle);
            }
        }

        private void CancelSequence()
        {
            if (_sequence == null || !_sequence.IsActive)
            {
                return;
            }

            _sequence.Cancel();
            actionState?.Complete(PlayerActionState.Attacking);
            AttackPhaseChanged?.Invoke(AttackPhase.Idle);
        }

        private AttackResult ResolveAttack()
        {
            if (Time.timeScale <= 0f || health != null && !health.IsAlive)
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
                recentAttackTarget = target as H4CombatHealth;
                recentAttackTargetAt = Time.time;
                GetComponent<H4CombatFeedback>()?.PlayAttack();
                BasicAttackSucceeded?.Invoke(LastAttackResult);
            }

            return LastAttackResult;
        }

        private ITargetable FindNearestTarget()
        {
            if (recentAttackTarget != null && recentAttackTarget.IsAlive && recentAttackTarget.IsTargetable
                && Time.time - recentAttackTargetAt <= RecentTargetContinuitySeconds
                && Vector2.Distance(transform.position, recentAttackTarget.transform.position) <= attackRange)
            {
                return recentAttackTarget;
            }

            var colliders = Physics2D.OverlapCircleAll(transform.position, attackRange, targetLayers);
            ITargetable nearestFacing = null;
            ITargetable nearestFallback = null;
            var nearestFacingDistance = float.PositiveInfinity;
            var nearestFallbackDistance = float.PositiveInfinity;
            var direction = GetComponent<H3PlayerController>()?.CurrentLookDirection
                ?? Vector2.right;
            foreach (var collider in colliders)
            {
                var target = collider.GetComponentInParent<H4CombatHealth>();
                if (target == null || target == health || !target.IsTargetable)
                {
                    continue;
                }

                var distance = Vector2.Distance(transform.position, target.transform.position);
                var toTarget = (Vector2)target.transform.position - (Vector2)transform.position;
                var isFacing = direction.sqrMagnitude <= 0.0001f || toTarget.sqrMagnitude <= 0.0001f
                    || Vector2.Dot(direction.normalized, toTarget.normalized) >= 0.05f;
                if (distance < nearestFallbackDistance)
                {
                    nearestFallback = target;
                    nearestFallbackDistance = distance;
                }

                if (isFacing && distance < nearestFacingDistance)
                {
                    nearestFacing = target;
                    nearestFacingDistance = distance;
                }
            }

            // Prefer the current look direction. If no target is in the
            // facing cone, retain the previous nearest-target behavior so
            // H3-H9 mission and combat flows remain compatible.
            return nearestFacing ?? nearestFallback;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
