using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Movement;
using Lumbre.Game.Domain.Navigation;
using Lumbre.Game.Client.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lumbre.Game.Client.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class H3PlayerController : MonoBehaviour
    {
        private static readonly Vector2 LogicalXAxis = new Vector2(0.8944272f, 0.4472136f);
        private static readonly Vector2 LogicalYAxis = new Vector2(-0.8944272f, 0.4472136f);

        [SerializeField] private H3PlayerInputReader inputReader;
        [SerializeField] private Transform respawnPoint;
        [SerializeField, Min(0.1f)] private float movementSpeed = 2.4f;
        [SerializeField, Min(0f)] private float acceleration = 64f;
        [SerializeField, Min(0f)] private float deceleration = 128f;
        [SerializeField, Range(0f, 0.95f)] private float inputDeadZone = 0.12f;
        [SerializeField, Min(0.01f)] private float analogResponse = 1f;
        [SerializeField, Min(0f)] private float outOfBoundsMargin = 1.5f;

        private Rigidbody2D _body;
        private MovementIntent _intent;
        private PlayerLocomotionModel _locomotion;
        private Vector2 _currentWorldVelocity;
        private Vector2 _lastLookDirection = Vector2.right;
        private H10PlayerActionStateController _actionState;

        public MovementIntent CurrentIntent => _intent;
        public Vector2 CurrentWorldDirection => _currentWorldVelocity;
        public Vector2 CurrentLookDirection => _lastLookDirection;
        public Vector2 CurrentWorldVelocity => _currentWorldVelocity;
        public float MaxSpeed => Mathf.Max(0f, movementSpeed);
        public float Acceleration => Mathf.Max(0f, acceleration);
        public float Deceleration => Mathf.Max(0f, deceleration);
        public float InputDeadZone => Mathf.Clamp01(inputDeadZone);
        public float AnalogResponse => Mathf.Max(0.01f, analogResponse);
        public Transform RespawnPoint
        {
            get => respawnPoint;
            set => respawnPoint = value;
        }

        public H3PlayerInputReader InputReader
        {
            get => inputReader;
            set => inputReader = value;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            inputReader ??= GetComponent<H3PlayerInputReader>();

            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _actionState = GetComponent<H10PlayerActionStateController>();
            _locomotion = CreateLocomotionModel();

            if (respawnPoint != null)
            {
                Respawn();
            }
        }

        private void Update()
        {
            _intent = inputReader == null
                ? new MovementIntent(0f, 0f)
                : inputReader.ReadIntent(inputDeadZone, analogResponse);

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Respawn();
            }

            if (IsOutsideGreybox(transform.position))
            {
                Respawn();
            }
        }

        private void FixedUpdate()
        {
            if (_locomotion == null)
            {
                _locomotion = CreateLocomotionModel();
            }

            var velocity = _locomotion.Tick(_intent, Time.fixedDeltaTime);
            _currentWorldVelocity = ToWorldVector(velocity.X, velocity.Y);
            if (_intent.IsMoving)
            {
                var direction = ToWorldDirection(_intent);
                if (direction.sqrMagnitude > 0.0001f)
                {
                    _lastLookDirection = direction.normalized;
                }
            }

            _actionState?.SetMoving(velocity.IsMoving);
            if (_currentWorldVelocity.sqrMagnitude < 0.0001f)
            {
                return;
            }

            _body.MovePosition(_body.position + _currentWorldVelocity * Time.fixedDeltaTime);
        }

        public void ConfigureLocomotion(float maxSpeed, float accelerationRate,
            float decelerationRate, float deadZone, float responseExponent)
        {
            movementSpeed = Mathf.Max(0f, maxSpeed);
            acceleration = Mathf.Max(0f, accelerationRate);
            deceleration = Mathf.Max(0f, decelerationRate);
            inputDeadZone = Mathf.Clamp(deadZone, 0f, 0.95f);
            analogResponse = Mathf.Max(0.01f, responseExponent);
            _locomotion = CreateLocomotionModel();
        }

        public void Respawn()
        {
            if (_body == null || respawnPoint == null)
            {
                return;
            }

            var position = respawnPoint.position;
            _body.position = new Vector2(position.x, position.y);
            _body.linearVelocity = Vector2.zero;
            _locomotion?.Reset();
            _currentWorldVelocity = Vector2.zero;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        public void RestoreSafePosition(Vector3 position)
        {
            if (_body == null)
            {
                return;
            }

            _body.position = new Vector2(position.x, position.y);
            _body.linearVelocity = Vector2.zero;
            _locomotion?.Reset();
            _currentWorldVelocity = Vector2.zero;
            transform.position = new Vector3(position.x, position.y, position.z);
        }

        public static Vector2 ToWorldDirection(MovementIntent intent)
        {
            var direction = ToWorldVector(intent.X, intent.Y);
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        public static Vector2 ToWorldVector(float x, float y)
        {
            return (LogicalXAxis * x) + (LogicalYAxis * y);
        }

        private PlayerLocomotionModel CreateLocomotionModel()
        {
            return new PlayerLocomotionModel(MaxSpeed, Acceleration, Deceleration);
        }

        private bool IsOutsideGreybox(Vector3 position)
        {
            var corners = new[]
            {
                GreyboxWorldCoordinates.ToWorld(new GridPosition(0, 0)),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(
                    ProjectConstants.GreyboxGridWidth - 1, 0)),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(
                    0, ProjectConstants.GreyboxGridHeight - 1)),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(
                    ProjectConstants.GreyboxGridWidth - 1,
                    ProjectConstants.GreyboxGridHeight - 1))
            };

            var minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x)
                - ProjectConstants.IsometricTileWidth * 0.5f - outOfBoundsMargin;
            var maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x)
                + ProjectConstants.IsometricTileWidth * 0.5f + outOfBoundsMargin;
            var minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y)
                - ProjectConstants.IsometricTileHeight * 0.5f - outOfBoundsMargin;
            var maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y)
                + ProjectConstants.IsometricTileHeight * 0.5f + outOfBoundsMargin;

            return position.x < minX || position.x > maxX
                || position.y < minY || position.y > maxY;
        }
    }
}
