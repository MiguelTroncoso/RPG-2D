using System;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Combat;
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
        private Transform _visualTransform;
        private Vector3 _visualBaseLocalPosition;
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
            _visualTransform = spriteRenderer != null ? spriteRenderer.transform : null;
            if (_visualTransform != null)
            {
                _visualBaseLocalPosition = _visualTransform.localPosition;
            }
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
                _playerCombat.AttackSequenceStarted += HandleAttackSequenceStarted;
            }

            if (_abilities != null)
            {
                _abilities.DefenseActivated += HandleDefense;
                _abilities.AreaAttackSequenceStarted += HandleAreaAttackSequenceStarted;
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
                _playerCombat.AttackSequenceStarted -= HandleAttackSequenceStarted;
            }

            if (_abilities != null)
            {
                _abilities.DefenseActivated -= HandleDefense;
                _abilities.AreaAttackSequenceStarted -= HandleAreaAttackSequenceStarted;
                _abilities.AreaAttackActivated -= HandleAreaAttack;
            }

            if (_enemyController != null)
            {
                _enemyController.StateChanged -= HandleEnemyState;
            }
        }

        private void Update()
        {
            var transient = Time.time < _transientUntil;
            if (!_dead && !transient && animator != null)
            {
                var moving = _playerController != null
                    ? _playerController.CurrentWorldDirection.sqrMagnitude > 0.01f
                    : _enemyController != null && (_enemyController.CurrentState == MordeluzAiState.Follow
                        || _enemyController.CurrentState == MordeluzAiState.Return);
                SetState(moving ? "Walk" : "Idle");
            }

            UpdatePlayerMotionPresentation(transient);
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
                PlayTransient("Damage", 0.18f);
            }
        }

        private void HandleDeath()
        {
            _dead = true;
            SetState("Death");
        }

        private void HandleBasicAttack(AttackResult result)
        {
            if (result.Succeeded)
            {
                PlayTransient("Attack", 0.28f);
            }
        }

        private void HandleAttackSequenceStarted()
        {
            if (_playerCombat == null)
            {
                return;
            }

            PlayTransient("Attack", _playerCombat.AttackAnticipationDuration
                + _playerCombat.AttackRecoveryDuration + 0.04f);
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

        private void HandleAreaAttackSequenceStarted()
        {
            if (_abilities == null)
            {
                return;
            }

            PlayTransient("Area", _abilities.AreaAnticipationDuration
                + _abilities.AreaRecoveryDuration + 0.08f);
        }

        private void UpdatePlayerMotionPresentation(bool transient)
        {
            if (_playerController == null || _visualTransform == null || _dead)
            {
                return;
            }

            var moving = !transient && _playerController.CurrentSpeedNormalized > 0.03f;
            var speed = _playerController.CurrentSpeedNormalized;
            var bob = moving ? Mathf.Sin(Time.time * 18f) * 0.025f * speed : 0f;
            var targetPosition = _visualBaseLocalPosition + Vector3.up * bob;
            var blend = 1f - Mathf.Exp(-20f * Time.deltaTime);
            _visualTransform.localPosition = Vector3.Lerp(_visualTransform.localPosition,
                targetPosition, blend);
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
