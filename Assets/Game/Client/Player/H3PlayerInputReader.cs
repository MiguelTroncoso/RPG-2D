using System;
using Lumbre.Game.Domain.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Lumbre.Game.Client.Player
{
    public sealed class H3PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actionsAsset;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string attackActionName = "Attack";
        [SerializeField] private string defenseActionName = "Defense";
        [SerializeField] private string areaAttackActionName = "AreaAttack";
        [SerializeField] private string interactActionName = "Interact";
        [SerializeField] private string equipActionName = "Equip";

        private InputActionMap _actionMap;
        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _defenseAction;
        private InputAction _areaAttackAction;
        private InputAction _interactAction;
        private InputAction _equipAction;
        private bool _ownsFallbackMap;
        private bool _callbacksBound;
        private bool _attackPressedThisFrame;
        private bool _defensePressedThisFrame;
        private bool _areaAttackPressedThisFrame;
        private bool _interactPressedThisFrame;
        private bool _equipPressedThisFrame;
        private bool _hasFocus = true;

        public InputActionAsset ActionsAsset
        {
            get => actionsAsset;
            set => actionsAsset = value;
        }

        /// <summary>
        /// Uses the existing Touch control scheme for presentation hints on mobile.
        /// Input still comes from the same action map and bindings as before.
        /// </summary>
        public bool IsTouchControlSchemeActive
        {
            get
            {
                if (!UnityEngine.Application.isMobilePlatform)
                {
                    return false;
                }

                if (actionsAsset == null)
                {
                    return true;
                }

                foreach (var scheme in actionsAsset.controlSchemes)
                {
                    if (string.Equals(scheme.name, "Touch", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void Awake()
        {
            ConfigureAction();
        }

        private void OnEnable()
        {
            ConfigureAction();
            BindButtonCallbacks();
            _actionMap?.Enable();
            _hasFocus = true;
        }

        private void OnDisable()
        {
            UnbindButtonCallbacks();
            _actionMap?.Disable();
            ClearLatchedActions();
        }

        private void OnDestroy()
        {
            if (_ownsFallbackMap)
            {
                _actionMap?.Dispose();
            }
        }

        public MovementIntent ReadIntent()
        {
            return ReadIntent(0.1f, 1f);
        }

        public MovementIntent ReadIntent(float deadZone, float responseExponent)
        {
            if (_moveAction == null || !_hasFocus || Time.timeScale <= 0f)
            {
                return new MovementIntent(0f, 0f);
            }

            var value = _moveAction.ReadValue<Vector2>();
            return new MovementIntent(value.x, value.y, deadZone, responseExponent);
        }

        /// <summary>
        /// Button actions are latched from InputAction.started instead of being
        /// sampled only from a MonoBehaviour Update. This preserves a touch
        /// press across the OnScreenControl one-frame queue boundary while
        /// keeping keyboard, gamepad and touch on the same action map.
        /// </summary>
        public bool AttackPressedThisFrame => _hasFocus && Time.timeScale > 0f
            && _attackPressedThisFrame;
        public bool DefensePressedThisFrame => _hasFocus && Time.timeScale > 0f
            && _defensePressedThisFrame;
        public bool AreaAttackPressedThisFrame => _hasFocus && Time.timeScale > 0f
            && _areaAttackPressedThisFrame;
        public bool InteractPressedThisFrame => _hasFocus && Time.timeScale > 0f
            && _interactPressedThisFrame;
        public bool EquipPressedThisFrame => _hasFocus && Time.timeScale > 0f
            && _equipPressedThisFrame;

        private void LateUpdate()
        {
            ClearLatchedActions();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;
            ClearLatchedActions();
            if (hasFocus)
            {
                return;
            }

            // OnScreenControl devices are virtual Gamepads. Reset only those
            // devices so a press held while Android suspends cannot stay stuck,
            // without touching a physical QA gamepad.
            foreach (var device in InputSystem.devices)
            {
                if (device.usages.Contains(new InternedString("OnScreen")))
                {
                    InputSystem.ResetDevice(device);
                }
            }
        }

        private void BindButtonCallbacks()
        {
            if (_callbacksBound)
            {
                return;
            }

            _callbacksBound = true;
            if (_attackAction != null) _attackAction.started += HandleAttackStarted;
            if (_defenseAction != null) _defenseAction.started += HandleDefenseStarted;
            if (_areaAttackAction != null) _areaAttackAction.started += HandleAreaAttackStarted;
            if (_interactAction != null) _interactAction.started += HandleInteractStarted;
            if (_equipAction != null) _equipAction.started += HandleEquipStarted;
        }

        private void UnbindButtonCallbacks()
        {
            if (!_callbacksBound)
            {
                return;
            }

            _callbacksBound = false;
            if (_attackAction != null) _attackAction.started -= HandleAttackStarted;
            if (_defenseAction != null) _defenseAction.started -= HandleDefenseStarted;
            if (_areaAttackAction != null) _areaAttackAction.started -= HandleAreaAttackStarted;
            if (_interactAction != null) _interactAction.started -= HandleInteractStarted;
            if (_equipAction != null) _equipAction.started -= HandleEquipStarted;
        }

        private void HandleAttackStarted(InputAction.CallbackContext _)
        {
            _attackPressedThisFrame = true;
        }
        private void HandleDefenseStarted(InputAction.CallbackContext _) => _defensePressedThisFrame = true;
        private void HandleAreaAttackStarted(InputAction.CallbackContext _) => _areaAttackPressedThisFrame = true;
        private void HandleInteractStarted(InputAction.CallbackContext _) => _interactPressedThisFrame = true;
        private void HandleEquipStarted(InputAction.CallbackContext _) => _equipPressedThisFrame = true;

        private void ClearLatchedActions()
        {
            _attackPressedThisFrame = false;
            _defensePressedThisFrame = false;
            _areaAttackPressedThisFrame = false;
            _interactPressedThisFrame = false;
            _equipPressedThisFrame = false;
        }

        private void ConfigureAction()
        {
            if (_moveAction != null)
            {
                return;
            }

            if (actionsAsset != null)
            {
                _actionMap = actionsAsset.FindActionMap(actionMapName, true);
                _moveAction = _actionMap.FindAction(moveActionName, true);
                _attackAction = _actionMap.FindAction(attackActionName, false);
                _defenseAction = _actionMap.FindAction(defenseActionName, false);
                _areaAttackAction = _actionMap.FindAction(areaAttackActionName, false);
                _interactAction = _actionMap.FindAction(interactActionName, false);
                _equipAction = _actionMap.FindAction(equipActionName, false);
                _ownsFallbackMap = false;
                return;
            }

            _actionMap = new InputActionMap(actionMapName);
            _moveAction = _actionMap.AddAction(
                moveActionName,
                InputActionType.Value,
                expectedControlLayout: "Vector2");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddBinding("<Gamepad>/leftStick");
            _attackAction = _actionMap.AddAction(
                attackActionName,
                InputActionType.Button,
                expectedControlLayout: "Button");
            _attackAction.AddBinding("<Keyboard>/space");
            _attackAction.AddBinding("<Gamepad>/buttonSouth");
            _defenseAction = _actionMap.AddAction(
                defenseActionName,
                InputActionType.Button,
                expectedControlLayout: "Button");
            _defenseAction.AddBinding("<Keyboard>/q");
            _defenseAction.AddBinding("<Gamepad>/buttonEast");
            _areaAttackAction = _actionMap.AddAction(
                areaAttackActionName,
                InputActionType.Button,
                expectedControlLayout: "Button");
            _areaAttackAction.AddBinding("<Keyboard>/e");
            _areaAttackAction.AddBinding("<Gamepad>/buttonWest");
            _interactAction = _actionMap.AddAction(
                interactActionName,
                InputActionType.Button,
                expectedControlLayout: "Button");
            _interactAction.AddBinding("<Keyboard>/f");
            _interactAction.AddBinding("<Gamepad>/buttonNorth");
            _equipAction = _actionMap.AddAction(
                equipActionName,
                InputActionType.Button,
                expectedControlLayout: "Button");
            _equipAction.AddBinding("<Keyboard>/g");
            _equipAction.AddBinding("<Gamepad>/leftShoulder");
            _ownsFallbackMap = true;
        }
    }
}
