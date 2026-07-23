using System;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Combat;
using Lumbre.Game.Domain.Constants;
using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    public sealed class H7CharacterView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private string characterId = "character";

        private H3PlayerController _playerController;
        private H4CombatHealth _health;
        private H4PlayerCombatController _playerCombat;
        private H4BPlayerAbilityController _abilities;
        private MordeluzController _enemyController;
        private string _currentState;
        private float _transientUntil;
        private bool _dead;

        public string CharacterId => characterId;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public Animator Animator => animator;
        public bool IsPresentationReady => spriteRenderer != null
            && spriteRenderer.sprite != null
            && animator != null
            && animator.runtimeAnimatorController != null;
        public string CurrentState => _currentState;

        public void Configure(string id, Sprite sprite, Animator targetAnimator)
        {
            characterId = string.IsNullOrWhiteSpace(id) ? "character" : id;
            spriteRenderer = spriteRenderer ?? GetComponentInChildren<SpriteRenderer>(true);
            animator = targetAnimator ?? GetComponent<Animator>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

        private void Awake()
        {
            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>(true);
            animator ??= GetComponent<Animator>();
            _playerController = GetComponent<H3PlayerController>();
            _health = GetComponent<H4CombatHealth>();
            _playerCombat = GetComponent<H4PlayerCombatController>();
            _abilities = GetComponent<H4BPlayerAbilityController>();
            _enemyController = GetComponent<MordeluzController>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.DamageTaken += HandleDamage;
                _health.Died += HandleDeath;
            }

            if (_playerCombat != null)
            {
                _playerCombat.BasicAttackSucceeded += HandleBasicAttack;
                _playerCombat.AttackPhaseChanged += HandleAttackPhase;
            }

            if (_abilities != null)
            {
                _abilities.DefenseActivated += HandleDefense;
                _abilities.AreaAttackActivated += HandleAreaAttack;
            }

            if (_enemyController != null)
            {
                _enemyController.StateChanged += HandleEnemyState;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.DamageTaken -= HandleDamage;
                _health.Died -= HandleDeath;
            }

            if (_playerCombat != null)
            {
                _playerCombat.BasicAttackSucceeded -= HandleBasicAttack;
                _playerCombat.AttackPhaseChanged -= HandleAttackPhase;
            }

            if (_abilities != null)
            {
                _abilities.DefenseActivated -= HandleDefense;
                _abilities.AreaAttackActivated -= HandleAreaAttack;
            }

            if (_enemyController != null)
            {
                _enemyController.StateChanged -= HandleEnemyState;
            }
        }

        private void Update()
        {
            if (_dead || animator == null)
            {
                return;
            }

            if (Time.time < _transientUntil)
            {
                animator.speed = 1f;
                return;
            }

            var moving = _playerController != null
                ? _playerController.CurrentWorldDirection.sqrMagnitude > 0.01f
                : _enemyController != null && (_enemyController.CurrentState == MordeluzAiState.Follow
                    || _enemyController.CurrentState == MordeluzAiState.Return);
            SetState(moving ? "Walk" : "Idle");
            SyncAnimatorSpeed(moving);
        }

        /// <summary>
        /// H10.3: while walking, the Animator playback speed follows the real
        /// physical speed so the semantic Walk state can never desync from
        /// the locomotion (proportional to analog intensity as well).
        /// </summary>
        private void SyncAnimatorSpeed(bool moving)
        {
            if (_playerController == null)
            {
                animator.speed = 1f;
                return;
            }

            if (!moving || !string.Equals(_currentState, "Walk", StringComparison.Ordinal))
            {
                animator.speed = 1f;
                return;
            }

            var normalized = Mathf.Clamp01(
                _playerController.CurrentWorldVelocity.magnitude
                / Mathf.Max(0.01f, _playerController.MaxSpeed));
            animator.speed = Mathf.Lerp(0.8f, 1.3f, normalized);
        }

        public void PlayTransient(string state, float duration)
        {
            if (_dead)
            {
                return;
            }

            SetState(state);
            _transientUntil = Mathf.Max(_transientUntil, Time.time + Mathf.Max(0f, duration));
        }

        public void SetState(string state)
        {
            if (animator == null || string.IsNullOrWhiteSpace(state) || !HasState(state))
            {
                return;
            }

            if (string.Equals(_currentState, state, StringComparison.Ordinal))
            {
                return;
            }

            _currentState = state;
            animator.Play(state, 0, 0f);
        }

        private bool HasState(string state)
        {
            return animator != null && animator.HasState(0, Animator.StringToHash("Base Layer." + state));
        }

        private void HandleDamage(DamageResult result)
        {
            if (!result.Killed)
            {
                // Enemies hold the damage state for the whole H10.3 stagger
                // window so the hit reaction stays on screen long enough.
                var duration = _enemyController != null
                    ? ProjectConstants.MordeluzStaggerSeconds
                    : 0.18f;
                PlayTransient("Damage", duration);
            }
        }

        private void HandleDeath()
        {
            _dead = true;
            SetState("Death");
        }

        private void HandleBasicAttack(AttackResult result)
        {
            // Input attacks are covered by HandleAttackPhase from windup to
            // recovery; this path remains for programmatic QA attacks only.
            if (result.Succeeded && (_playerCombat == null
                || _playerCombat.CurrentAttackPhase == AttackPhase.Idle))
            {
                PlayTransient("Attack", 0.28f);
            }
        }

        private void HandleAttackPhase(AttackPhase phase)
        {
            if (phase == AttackPhase.Windup && _playerCombat != null)
            {
                PlayTransient("Attack", _playerCombat.AttackSequenceSeconds);
            }
        }

        private void HandleDefense(AbilityResult result)
        {
            if (result.Succeeded)
            {
                PlayTransient("Defense", 0.45f);
            }
        }

        private void HandleAreaAttack(AbilityResult result)
        {
            if (result.Succeeded)
            {
                PlayTransient("Area", 0.35f);
            }
        }

        private void HandleEnemyState(MordeluzAiTransition transition)
        {
            if (!transition.Changed || _dead)
            {
                return;
            }

            switch (transition.Current)
            {
                case MordeluzAiState.Attack:
                    PlayTransient("Attack", 0.25f);
                    break;
                case MordeluzAiState.Follow:
                case MordeluzAiState.Return:
                    SetState("Walk");
                    break;
                case MordeluzAiState.Idle:
                    SetState("Idle");
                    break;
            }
        }
    }
}
