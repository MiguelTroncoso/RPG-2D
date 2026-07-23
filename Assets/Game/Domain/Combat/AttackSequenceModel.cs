using System;

namespace Lumbre.Game.Domain.Combat
{
    public enum AttackPhase
    {
        Idle = 0,
        Windup = 1,
        Strike = 2,
        Impact = 3,
        Recovery = 4
    }

    public readonly struct AttackSequenceTick
    {
        public AttackSequenceTick(bool phaseChanged, AttackPhase phase,
            bool impactStarted, bool completed)
        {
            PhaseChanged = phaseChanged;
            Phase = phase;
            ImpactStarted = impactStarted;
            Completed = completed;
        }

        public bool PhaseChanged { get; }
        public AttackPhase Phase { get; }

        /// <summary>True exactly once per sequence, at the visual impact.</summary>
        public bool ImpactStarted { get; }
        public bool Completed { get; }
    }

    /// <summary>
    /// Timed Windup → Strike → Impact → Recovery sequence for the basic
    /// attack. Damage must be applied only when <see cref="AttackSequenceTick.ImpactStarted"/>
    /// reports true, which happens exactly at the start of the visual impact —
    /// never at button press time. Pure C# so tests can drive time directly.
    /// </summary>
    public sealed class AttackSequenceModel
    {
        private readonly float _windupSeconds;
        private readonly float _strikeSeconds;
        private readonly float _impactSeconds;
        private readonly float _recoverySeconds;

        private float _startTime;
        private bool _impactFired;

        public AttackSequenceModel(float windupSeconds, float strikeSeconds,
            float impactSeconds, float recoverySeconds)
        {
            _windupSeconds = Math.Max(0.01f, windupSeconds);
            _strikeSeconds = Math.Max(0.01f, strikeSeconds);
            _impactSeconds = Math.Max(0.01f, impactSeconds);
            _recoverySeconds = Math.Max(0.01f, recoverySeconds);
        }

        public AttackPhase Phase { get; private set; } = AttackPhase.Idle;
        public bool IsActive => Phase != AttackPhase.Idle;
        public float TotalSeconds => _windupSeconds + _strikeSeconds
            + _impactSeconds + _recoverySeconds;
        public float ImpactDelaySeconds => _windupSeconds + _strikeSeconds;

        public bool TryStart(float time)
        {
            if (IsActive)
            {
                return false;
            }

            _startTime = time;
            _impactFired = false;
            Phase = AttackPhase.Windup;
            return true;
        }

        public AttackSequenceTick Tick(float time)
        {
            if (!IsActive)
            {
                return new AttackSequenceTick(false, AttackPhase.Idle, false, false);
            }

            var elapsed = time - _startTime;
            var nextPhase = PhaseAt(elapsed);
            var changed = nextPhase != Phase;
            var impactStarted = false;
            // A large delta time may skip the whole impact window; the impact
            // event must still fire exactly once so damage timing never drops.
            if (!_impactFired && elapsed >= ImpactDelaySeconds)
            {
                _impactFired = true;
                impactStarted = true;
            }

            Phase = nextPhase;
            var completed = false;
            if (elapsed >= TotalSeconds)
            {
                Phase = AttackPhase.Idle;
                completed = true;
            }

            return new AttackSequenceTick(changed, Phase, impactStarted, completed);
        }

        /// <summary>Normalized 0..1 progress inside the current phase.</summary>
        public float PhaseProgress(float time)
        {
            if (!IsActive)
            {
                return 0f;
            }

            var elapsed = Math.Max(0f, time - _startTime);
            float phaseStart;
            float phaseDuration;
            switch (Phase)
            {
                case AttackPhase.Windup:
                    phaseStart = 0f;
                    phaseDuration = _windupSeconds;
                    break;
                case AttackPhase.Strike:
                    phaseStart = _windupSeconds;
                    phaseDuration = _strikeSeconds;
                    break;
                case AttackPhase.Impact:
                    phaseStart = _windupSeconds + _strikeSeconds;
                    phaseDuration = _impactSeconds;
                    break;
                default:
                    phaseStart = _windupSeconds + _strikeSeconds + _impactSeconds;
                    phaseDuration = _recoverySeconds;
                    break;
            }

            var progress = (elapsed - phaseStart) / phaseDuration;
            return progress < 0f ? 0f : progress > 1f ? 1f : progress;
        }

        public void Cancel()
        {
            Phase = AttackPhase.Idle;
            _impactFired = false;
        }

        private AttackPhase PhaseAt(float elapsed)
        {
            if (elapsed < _windupSeconds)
            {
                return AttackPhase.Windup;
            }

            if (elapsed < _windupSeconds + _strikeSeconds)
            {
                return AttackPhase.Strike;
            }

            if (elapsed < _windupSeconds + _strikeSeconds + _impactSeconds)
            {
                return AttackPhase.Impact;
            }

            return AttackPhase.Recovery;
        }
    }
}
