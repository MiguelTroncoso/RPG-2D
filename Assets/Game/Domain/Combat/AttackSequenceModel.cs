using System;

namespace Lumbre.Game.Domain.Combat
{
    /// <summary>
    /// Deterministic timing gate for a presentation-driven attack. It does not
    /// apply damage or know about Unity; it only guarantees one impact per
    /// accepted sequence.
    /// </summary>
    public sealed class AttackSequenceModel
    {
        private readonly float _anticipationDuration;
        private readonly float _recoveryDuration;
        private float _phaseEndsAt;
        private bool _impactConsumed;

        public AttackSequenceModel(float anticipationDuration, float recoveryDuration)
        {
            _anticipationDuration = Math.Max(0f, anticipationDuration);
            _recoveryDuration = Math.Max(0f, recoveryDuration);
        }

        public AttackSequencePhase Phase { get; private set; } = AttackSequencePhase.Idle;
        public float AnticipationDuration => _anticipationDuration;
        public float RecoveryDuration => _recoveryDuration;
        public bool IsActive => Phase != AttackSequencePhase.Idle;

        public bool TryBegin(float now)
        {
            if (IsActive)
            {
                return false;
            }

            Phase = AttackSequencePhase.Anticipation;
            _phaseEndsAt = now + _anticipationDuration;
            _impactConsumed = false;
            return true;
        }

        public bool TryEnterImpact(float now)
        {
            if (Phase != AttackSequencePhase.Anticipation || now < _phaseEndsAt)
            {
                return false;
            }

            Phase = AttackSequencePhase.Impact;
            return true;
        }

        public bool TryConsumeImpact()
        {
            if (Phase != AttackSequencePhase.Impact || _impactConsumed)
            {
                return false;
            }

            _impactConsumed = true;
            return true;
        }

        public bool TryEnterRecovery(float now)
        {
            if (Phase != AttackSequencePhase.Impact || !_impactConsumed)
            {
                return false;
            }

            Phase = AttackSequencePhase.Recovery;
            _phaseEndsAt = now + _recoveryDuration;
            return true;
        }

        public bool TryComplete(float now)
        {
            if (Phase != AttackSequencePhase.Recovery || now < _phaseEndsAt)
            {
                return false;
            }

            Reset();
            return true;
        }

        public void Reset()
        {
            Phase = AttackSequencePhase.Idle;
            _phaseEndsAt = 0f;
            _impactConsumed = false;
        }
    }
}
