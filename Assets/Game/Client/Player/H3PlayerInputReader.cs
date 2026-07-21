using Lumbre.Game.Domain.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

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

        public InputActionAsset ActionsAsset
        {
            get => actionsAsset;
            set => actionsAsset = value;
        }

        private void Awake()
        {
            ConfigureAction();
        }

        private void OnEnable()
        {
            ConfigureAction();
            _actionMap?.Enable();
        }

        private void OnDisable()
        {
            _actionMap?.Disable();
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
            if (_moveAction == null)
            {
                return new MovementIntent(0f, 0f);
            }

            var value = _moveAction.ReadValue<Vector2>();
            return new MovementIntent(value.x, value.y);
        }

        public bool AttackPressedThisFrame => _attackAction != null
            && _attackAction.WasPressedThisFrame();
        public bool DefensePressedThisFrame => _defenseAction != null
            && _defenseAction.WasPressedThisFrame();
        public bool AreaAttackPressedThisFrame => _areaAttackAction != null
            && _areaAttackAction.WasPressedThisFrame();
        public bool InteractPressedThisFrame => _interactAction != null
            && _interactAction.WasPressedThisFrame();
        public bool EquipPressedThisFrame => _equipAction != null
            && _equipAction.WasPressedThisFrame();

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
