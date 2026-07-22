using Lumbre.Game.Client.Combat;
using Lumbre.Game.Domain.Movement;
using UnityEngine;

namespace Lumbre.Game.Client.Player
{
    /// <summary>
    /// Unity adapter for the domain action gate. It owns no combat rules; it
    /// only synchronizes pause/death with the shared input boundary.
    /// </summary>
    public sealed class H10PlayerActionStateController : MonoBehaviour
    {
        private PlayerActionStateModel _model;
        private H4CombatHealth _health;

        public PlayerActionState CurrentState => _model?.Current ?? PlayerActionState.Idle;

        private void Awake()
        {
            _model = new PlayerActionStateModel();
            _health = GetComponent<H4CombatHealth>();
            if (_health != null)
            {
                _health.Died += HandleDied;
            }
        }

        private void Update()
        {
            if (_model == null)
            {
                _model = new PlayerActionStateModel();
            }

            if (_health != null && !_health.IsAlive)
            {
                _model.SetDead(true);
                return;
            }

            _model.SetPaused(Time.timeScale <= 0f);
        }

        public bool TryBegin(PlayerActionState state)
        {
            if (_model == null)
            {
                _model = new PlayerActionStateModel();
            }

            if (Time.timeScale <= 0f)
            {
                _model.SetPaused(true);
                return false;
            }

            return _model.TryBegin(state);
        }

        public void Complete(PlayerActionState state)
        {
            _model?.Complete(state);
        }

        public void SetMoving(bool moving)
        {
            _model?.SetMoving(moving);
        }

        public void SetDead(bool dead)
        {
            _model?.SetDead(dead);
        }

        private void HandleDied()
        {
            _model?.SetDead(true);
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Died -= HandleDied;
            }
        }
    }
}
