using System;
using System.Collections;
using Lumbre.Game.Domain.Combat;
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
        private float attackAnticipationDuration = 0.1f;
        private float attackRecoveryDuration = 0.18f;

        [NonSerialized] private H10PlayerActionStateController actionState;
        [NonSerialized] private H4CombatHealth recentAttackTarget;
        [NonSerialized] private float recentAttackTargetAt = float.NegativeInfinity;
        [NonSerialized] private AttackSequenceModel attackSequence;
        [NonSerialized] private Coroutine attackRoutine;
        private const float RecentTargetContinuitySeconds = 1.5f;

        public AttackResult LastAttackResult { get; private set; }
        public float AttackRange => attackRange;
        public float AttackAnticipationDuration => Mathf.Max(0f, attackAnticipationDuration);
        public float AttackRecoveryDuration => Mathf.Max(0f, attackRecoveryDuration);
        public AttackSequencePhase AttackSequencePhase => attackSequence?.Phase
            ?? AttackSequencePhase.Idle;
        public bool IsAttackSequenceActive => attackSequence?.IsActive == true;
        public event Action AttackSequenceStarted;
        public event Action<AttackResult> AttackResolved;
        public event Action<AttackResult> BasicAttackSucceeded;

        public void Configure(H3PlayerInputReader reader, H4BasicAttacker basicAttacker,
            H4CombatHealth combatHealth, LayerMask layers, float range)
        {
            inputReader = reader;
            attacker = basicAttacker;
            health = combatHealth;
            targetLayers = layers;
            attackRange = Mathf.Max(0.1f, range);
            InitializeSequence();
        }

        private void Awake()
        {
            inputReader ??= GetComponent<H3PlayerInputReader>();
            attacker ??= GetComponent<H4BasicAttacker>();
            health ??= GetComponent<H4CombatHealth>();
            actionState ??= GetComponent<H10PlayerActionStateController>();
            InitializeSequence();
        }

        private void Update()
        {
            if (inputReader != null && inputReader.AttackPressedThisFrame)
            {
                TryBasicAttackFromInput();
            }
        }

        public AttackResult TryBasicAttack()
        {
            return TryBasicAttackCore();
        }

        public void ConfigureTiming(float anticipationDuration, float recoveryDuration)
        {
            attackAnticipationDuration = Mathf.Max(0f, anticipationDuration);
            attackRecoveryDuration = Mathf.Max(0f, recoveryDuration);
            InitializeSequence();
        }

        private void TryBasicAttackFromInput()
        {
            if (attackRoutine != null || actionState != null
                && !actionState.TryBegin(PlayerActionState.Attacking))
            {
                PublishFailure(AttackResultCode.InvalidTarget);
                return;
            }

            var target = FindNearestTarget();
            if (target == null)
            {
                CompleteInputAction();
                PublishFailure(AttackResultCode.InvalidTarget);
                return;
            }

            if (attacker == null || !attacker.CanAttack(Time.time))
            {
                CompleteInputAction();
                PublishFailure(AttackResultCode.Cooldown);
                return;
            }

            InitializeSequence();
            if (!attackSequence.TryBegin(Time.time))
            {
                CompleteInputAction();
                PublishFailure(AttackResultCode.InvalidTarget);
                return;
            }

            attackRoutine = StartCoroutine(AttackSequence(target));
        }

        private AttackResult TryBasicAttackCore()
        {
            if (Time.timeScale <= 0f || health != null && !health.IsAlive)
            {
                PublishFailure(AttackResultCode.InvalidTarget);
                return LastAttackResult;
            }

            var target = FindNearestTarget();
            if (target == null)
            {
                PublishFailure(AttackResultCode.InvalidTarget);
                return LastAttackResult;
            }

            return ResolveAttack(target);
        }

        private IEnumerator AttackSequence(ITargetable target)
        {
            var feedback = GetComponent<H4CombatFeedback>();
            try
            {
                AttackSequenceStarted?.Invoke();
                feedback?.PlayAttackAnticipation();
                yield return new WaitForSeconds(AttackAnticipationDuration);

                if (!attackSequence.TryEnterImpact(Time.time))
                {
                    PublishFailure(AttackResultCode.InvalidTarget);
                    yield break;
                }

                AttackResult result;
                var resultPublished = false;
                if (Time.timeScale <= 0f || health != null && !health.IsAlive)
                {
                    PublishFailure(AttackResultCode.InvalidTarget);
                    result = LastAttackResult;
                    resultPublished = true;
                }
                else if (!IsTargetValidAtImpact(target))
                {
                    var code = target?.Health?.IsAlive == false
                        ? AttackResultCode.TargetDead
                        : AttackResultCode.InvalidTarget;
                    PublishFailure(code);
                    result = LastAttackResult;
                    resultPublished = true;
                }
                else if (!attackSequence.TryConsumeImpact())
                {
                    PublishFailure(AttackResultCode.InvalidTarget);
                    result = LastAttackResult;
                    resultPublished = true;
                }
                else
                {
                    result = ResolveAttack(target);
                    resultPublished = true;
                }

                if (!result.Succeeded && !resultPublished)
                {
                    LastAttackResult = result;
                    AttackResolved?.Invoke(result);
                }

                attackSequence.TryEnterRecovery(Time.time);
                yield return new WaitForSeconds(AttackRecoveryDuration);
                attackSequence.TryComplete(Time.time);
            }
            finally
            {
                attackSequence.Reset();
                attackRoutine = null;
                CompleteInputAction();
            }
        }

        private AttackResult ResolveAttack(ITargetable target)
        {
            LastAttackResult = attacker == null
                ? AttackResult.Failure(AttackResultCode.InvalidTarget)
                : attacker.TryAttack(target, Time.time);
            AttackResolved?.Invoke(LastAttackResult);
            if (!LastAttackResult.Succeeded)
            {
                return LastAttackResult;
            }

            recentAttackTarget = target as H4CombatHealth;
            recentAttackTargetAt = Time.time;
            GetComponent<H4CombatFeedback>()?.PlayAttack();
            BasicAttackSucceeded?.Invoke(LastAttackResult);
            return LastAttackResult;
        }

        private void PublishFailure(AttackResultCode code)
        {
            LastAttackResult = AttackResult.Failure(code);
            AttackResolved?.Invoke(LastAttackResult);
        }

        private void CompleteInputAction()
        {
            actionState?.Complete(PlayerActionState.Attacking);
        }

        private bool IsTargetValidAtImpact(ITargetable target)
        {
            var combatHealth = target as H4CombatHealth;
            return combatHealth != null && combatHealth.IsTargetable
                && Vector2.Distance(transform.position, combatHealth.transform.position) <= attackRange;
        }

        private void InitializeSequence()
        {
            attackSequence = new AttackSequenceModel(AttackAnticipationDuration,
                AttackRecoveryDuration);
        }

        private void OnDisable()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            attackSequence?.Reset();
            CompleteInputAction();
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
