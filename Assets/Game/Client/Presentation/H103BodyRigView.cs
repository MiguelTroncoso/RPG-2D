using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Animation;
using Lumbre.Game.Domain.Combat;
using UnityEngine;

namespace Lumbre.Game.Client.Presentation
{
    /// <summary>
    /// Drives the cutout body rig sliced from the single bastion-brasa
    /// illustration. Limb rotations come from the pure
    /// <see cref="PlayerBodyPoseModel"/> (stride synced to physical speed) and
    /// the attack overlay follows the combat controller's visible phases, so
    /// the body always moves with the simulation instead of sliding.
    /// </summary>
    public sealed class H103BodyRigView : MonoBehaviour
    {
        public const string RigRootName = "H10_3_Rig";
        public const float PixelsPerUnit = 512f;

        [SerializeField] private Transform rigRoot;
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private Transform armSword;
        [SerializeField] private Transform shield;
        [SerializeField] private Transform legFront;
        [SerializeField] private Transform legBack;
        [SerializeField] private Transform cape;
        [SerializeField, Range(0f, 1f)] private float facingFlipThreshold = 0.2f;

        private H3PlayerController _playerController;
        private H4PlayerCombatController _combatController;
        private H4CombatHealth _health;
        private PlayerBodyPoseModel _poseModel;
        private Vector3 _torsoBasePosition;
        private Vector3 _legFrontBasePosition;
        private Vector3 _legBackBasePosition;
        private float _facingSign = 1f;
        private float _attackWeight;
        private bool _basePositionsCaptured;

        public bool IsRigReady => rigRoot != null && torso != null && head != null
            && armSword != null && shield != null && legFront != null
            && legBack != null && cape != null;
        public float FacingSign => _facingSign;
        public float CurrentAttackWeight => _attackWeight;
        public PlayerBodyPoseModel PoseModel => _poseModel;

        public void Configure(Transform root)
        {
            rigRoot = root;
            if (rigRoot == null)
            {
                return;
            }

            torso = rigRoot.Find("Rig_Torso");
            cape = rigRoot.Find("Rig_Cape");
            legFront = rigRoot.Find("Rig_LegFront");
            legBack = rigRoot.Find("Rig_LegBack");
            if (torso != null)
            {
                head = torso.Find("Rig_Head");
                armSword = torso.Find("Rig_ArmSword");
                shield = torso.Find("Rig_Shield");
            }
        }

        private void Awake()
        {
            _playerController = GetComponent<H3PlayerController>();
            _combatController = GetComponent<H4PlayerCombatController>();
            _health = GetComponent<H4CombatHealth>();
            _poseModel = new PlayerBodyPoseModel();
            if (rigRoot == null)
            {
                Configure(transform.Find(RigRootName));
            }

            CaptureBasePositions();
        }

        private void LateUpdate()
        {
            if (!IsRigReady || _poseModel == null)
            {
                return;
            }

            if (_health != null && !_health.IsAlive)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            var speed = 0f;
            var distanceDelta = 0f;
            if (_playerController != null)
            {
                var velocity = _playerController.CurrentWorldVelocity;
                var maxSpeed = Mathf.Max(0.01f, _playerController.MaxSpeed);
                speed = Mathf.Clamp01(velocity.magnitude / maxSpeed);
                distanceDelta = velocity.magnitude * deltaTime;
                UpdateFacing(_playerController.CurrentLookDirection);
            }

            var locomotion = _poseModel.Tick(speed, distanceDelta, deltaTime);
            var pose = ApplyAttackOverlay(locomotion, deltaTime);
            ApplyPose(pose);
        }

        private PlayerBodyPose ApplyAttackOverlay(PlayerBodyPose locomotion,
            float deltaTime)
        {
            var phase = _combatController != null
                ? _combatController.CurrentAttackPhase
                : AttackPhase.Idle;
            var targetWeight = phase == AttackPhase.Idle ? 0f : 1f;
            _attackWeight = Mathf.MoveTowards(_attackWeight, targetWeight,
                deltaTime * 12f);
            if (_attackWeight <= 0f)
            {
                return locomotion;
            }

            var progress = _combatController != null
                ? _combatController.CurrentAttackPhaseProgress
                : 0f;
            var attackPose = PlayerAttackPoseCurve.Sample(phase, progress);
            return PlayerBodyPose.Blend(locomotion, attackPose, _attackWeight);
        }

        private void ApplyPose(PlayerBodyPose pose)
        {
            torso.localRotation = Quaternion.Euler(0f, 0f, pose.TorsoAngle);
            head.localRotation = Quaternion.Euler(0f, 0f, pose.HeadAngle);
            armSword.localRotation = Quaternion.Euler(0f, 0f, pose.ArmAngle);
            shield.localRotation = Quaternion.Euler(0f, 0f, pose.ShieldAngle);
            legFront.localRotation = Quaternion.Euler(0f, 0f, pose.FrontLegAngle);
            legBack.localRotation = Quaternion.Euler(0f, 0f, pose.BackLegAngle);
            cape.localRotation = Quaternion.Euler(0f, 0f, pose.CapeAngle);

            torso.localPosition = _torsoBasePosition + new Vector3(
                pose.HipOffsetX / PixelsPerUnit, pose.HipOffsetY / PixelsPerUnit, 0f);
            legFront.localPosition = _legFrontBasePosition
                + new Vector3(0f, pose.FrontLegLift / PixelsPerUnit, 0f);
            legBack.localPosition = _legBackBasePosition
                + new Vector3(0f, pose.BackLegLift / PixelsPerUnit, 0f);

            var scale = rigRoot.localScale;
            var magnitude = Mathf.Abs(scale.x) < 0.0001f ? 1f : Mathf.Abs(scale.x);
            rigRoot.localScale = new Vector3(magnitude * _facingSign, scale.y, scale.z);
        }

        private void UpdateFacing(Vector2 lookDirection)
        {
            // Hysteresis keeps the facing stable through diagonals and small
            // jitters: only flip after the horizontal component clearly
            // crosses the threshold in the opposite direction.
            if (lookDirection.x > facingFlipThreshold)
            {
                _facingSign = 1f;
            }
            else if (lookDirection.x < -facingFlipThreshold)
            {
                _facingSign = -1f;
            }
        }

        private void CaptureBasePositions()
        {
            if (_basePositionsCaptured || !IsRigReady)
            {
                return;
            }

            _basePositionsCaptured = true;
            _torsoBasePosition = torso.localPosition;
            _legFrontBasePosition = legFront.localPosition;
            _legBackBasePosition = legBack.localPosition;
        }
    }
}
