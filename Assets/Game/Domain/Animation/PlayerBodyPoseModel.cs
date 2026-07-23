using System;

namespace Lumbre.Game.Domain.Animation
{
    /// <summary>
    /// Deterministic locomotion pose generator for the cutout player rig.
    /// The stride phase advances with the distance actually travelled, so the
    /// foot cadence always matches the physical ground speed and the sprite
    /// can never "skate". Amplitudes scale with the normalized speed, which is
    /// itself proportional to the analog joystick intensity. No Unity types:
    /// the model stays reusable and unit-testable outside the engine.
    /// </summary>
    public sealed class PlayerBodyPoseModel
    {
        private const float TwoPi = (float)(Math.PI * 2.0);

        private readonly float _strideLength;
        private readonly float _maxLegSwingDegrees;
        private readonly float _maxArmSwingDegrees;
        private readonly float _maxTorsoSwayDegrees;
        private readonly float _maxLeanDegrees;
        private readonly float _maxBobPixels;
        private readonly float _maxStepLiftPixels;
        private readonly float _breathCycleSeconds;

        private float _stridePhase;
        private float _breathPhase;
        private float _amplitude;

        public PlayerBodyPoseModel(
            float strideLength = 0.92f,
            float maxLegSwingDegrees = 10f,
            float maxArmSwingDegrees = 6.5f,
            float maxTorsoSwayDegrees = 2.2f,
            float maxLeanDegrees = 2.5f,
            float maxBobPixels = 7f,
            float maxStepLiftPixels = 6f,
            float breathCycleSeconds = 3.4f)
        {
            _strideLength = Math.Max(0.05f, strideLength);
            _maxLegSwingDegrees = maxLegSwingDegrees;
            _maxArmSwingDegrees = maxArmSwingDegrees;
            _maxTorsoSwayDegrees = maxTorsoSwayDegrees;
            _maxLeanDegrees = maxLeanDegrees;
            _maxBobPixels = maxBobPixels;
            _maxStepLiftPixels = maxStepLiftPixels;
            _breathCycleSeconds = Math.Max(0.1f, breathCycleSeconds);
        }

        public float StridePhase => _stridePhase;
        public float Amplitude => _amplitude;

        /// <param name="normalizedSpeed">Current speed / max speed, 0..1.</param>
        /// <param name="distanceDelta">World distance travelled this tick.</param>
        /// <param name="deltaTime">Tick duration in seconds.</param>
        public PlayerBodyPose Tick(float normalizedSpeed, float distanceDelta,
            float deltaTime)
        {
            var speed = Clamp01(normalizedSpeed);
            var safeDelta = Math.Max(0f, deltaTime);
            _stridePhase += Math.Max(0f, distanceDelta) / _strideLength * TwoPi;
            _stridePhase %= TwoPi;
            _breathPhase += safeDelta / _breathCycleSeconds * TwoPi;
            _breathPhase %= TwoPi;

            // Smooth the walk amplitude so a sudden stop settles instead of
            // snapping; rises fast (respond to input), falls a bit slower.
            var target = SwingCurve(speed);
            var rate = target > _amplitude ? 10f : 6f;
            _amplitude = MoveTowards(_amplitude, target, rate * safeDelta);

            var swing = (float)Math.Sin(_stridePhase);
            var lift = (float)Math.Cos(_stridePhase);
            var amp = _amplitude;
            var breath = (float)Math.Sin(_breathPhase);

            var frontLeg = _maxLegSwingDegrees * swing * amp;
            var backLeg = -_maxLegSwingDegrees * swing * amp;
            var torso = _maxTorsoSwayDegrees * swing * amp
                - _maxLeanDegrees * speed
                + 0.5f * breath * (1f - amp);
            var head = -0.55f * torso + 0.35f * breath * (1f - amp);
            var arm = -_maxArmSwingDegrees * swing * amp
                + 1.2f * breath * (1f - amp);
            var shield = 1.6f * swing * amp;
            var cape = -4.5f * swing * amp - 3f * speed
                + 1.4f * breath * (1f - amp);
            var bob = -_maxBobPixels * Math.Abs(lift) * amp;
            var frontLift = Math.Max(0f, -(float)Math.Sin(_stridePhase))
                * _maxStepLiftPixels * amp;
            var backLift = Math.Max(0f, (float)Math.Sin(_stridePhase))
                * _maxStepLiftPixels * amp;

            return new PlayerBodyPose(torso, head, arm, shield, frontLeg,
                backLeg, cape, 0f, bob, frontLift, backLift);
        }

        public void Reset()
        {
            _stridePhase = 0f;
            _breathPhase = 0f;
            _amplitude = 0f;
        }

        private static float SwingCurve(float speed)
        {
            // Full body motion arrives quickly but still scales with slow
            // joystick intensities instead of jumping to the maximum.
            return Math.Min(1f, (float)Math.Pow(speed, 0.7) * 1.35f);
        }

        private static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + Math.Sign(target - current) * maxDelta;
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
