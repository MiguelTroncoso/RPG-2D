using Lumbre.Game.Domain.Combat;
using UnityEngine;

namespace Lumbre.Game.Client.Combat
{
    public sealed class MordeluzResonanteController : MordeluzController
    {
        [SerializeField, Min(1)] private int waveDamage = 18;
        [SerializeField, Min(0.1f)] private float waveRadius = 1.8f;
        [SerializeField, Min(0f)] private float waveWindup = 0.8f;
        [SerializeField, Min(0f)] private float waveCooldown = 2.4f;
        [SerializeField] private H4BWaveTelegraph telegraph;
        [SerializeField] private H4BAbilityFeedback feedback;

        private ResonantWaveAttackModel _wave;
        private ICombatTimeSource _timeSource;

        public bool IsWaveTelegraphActive => _wave != null && _wave.IsTelegraphActive;
        public float WaveCooldownRemaining => _wave?.CooldownRemaining ?? 0f;

        protected override bool HasPendingAttack => _wave != null && _wave.IsTelegraphActive;

        protected override void Awake()
        {
            base.Awake();
            telegraph ??= GetComponent<H4BWaveTelegraph>();
            feedback ??= GetComponent<H4BAbilityFeedback>();
            _timeSource ??= new UnityCombatTimeSource();
            _wave = new ResonantWaveAttackModel(_timeSource, waveWindup, waveCooldown);
            if (HealthComponent != null)
            {
                HealthComponent.Died += HandleResonantDeath;
            }
        }

        protected override void OnDestroy()
        {
            if (HealthComponent != null)
            {
                HealthComponent.Died -= HandleResonantDeath;
            }

            telegraph?.Hide();
            base.OnDestroy();
        }

        public void ConfigureElite(int damage, float radius, float windup, float cooldown,
            H4BWaveTelegraph waveTelegraph, H4BAbilityFeedback abilityFeedback)
        {
            waveDamage = Mathf.Max(1, damage);
            waveRadius = Mathf.Max(0.1f, radius);
            waveWindup = Mathf.Max(0f, windup);
            waveCooldown = Mathf.Max(0f, cooldown);
            telegraph = waveTelegraph;
            feedback = abilityFeedback;
            if (UnityEngine.Application.isPlaying)
            {
                _timeSource ??= new UnityCombatTimeSource();
                _wave = new ResonantWaveAttackModel(_timeSource, waveWindup, waveCooldown);
            }
        }

        private void HandleResonantDeath()
        {
            telegraph?.Hide();
        }

        protected override void PerformAttack(H4CombatHealth targetHealth, float distanceToTarget)
        {
            if (_wave == null)
            {
                return;
            }

            if (!_wave.IsTelegraphActive)
            {
                var started = _wave.TryBegin();
                if (started.Succeeded)
                {
                    telegraph?.Show(waveRadius);
                    feedback?.PlayWaveWarning();
                }

                return;
            }

            if (!_wave.CanResolve)
            {
                return;
            }

            var resolved = _wave.TryResolve();
            telegraph?.Hide();
            if (!resolved.Succeeded)
            {
                return;
            }

            feedback?.PlayWaveImpact();
            if (distanceToTarget <= waveRadius && targetHealth.IsTargetable)
            {
                targetHealth.ReceiveDamage(new CombatDamage(waveDamage, "mordeluz-resonant-wave"));
            }
        }
    }
}
