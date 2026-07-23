using Lumbre.Game.Domain.Combat;

namespace Lumbre.Game.Domain.Animation
{
    /// <summary>
    /// Key poses for each visible attack phase of the cutout rig. Windup pulls
    /// the sword arm and torso back, Strike sweeps the sword forward and up,
    /// Impact holds the extended contact pose, Recovery eases back to neutral.
    /// </summary>
    public static class PlayerAttackPoseCurve
    {
        private static readonly PlayerBodyPose Windup = new PlayerBodyPose(
            torsoAngle: 5.5f, headAngle: 2.5f, armAngle: 22f, shieldAngle: -2f,
            frontLegAngle: 1.5f, backLegAngle: -1.5f, capeAngle: -5f,
            hipOffsetX: -5f, hipOffsetY: -3f, frontLegLift: 0f, backLegLift: 0f);

        private static readonly PlayerBodyPose Strike = new PlayerBodyPose(
            torsoAngle: -8f, headAngle: -3.5f, armAngle: -52f, shieldAngle: 3f,
            frontLegAngle: -2f, backLegAngle: 2f, capeAngle: 10f,
            hipOffsetX: 14f, hipOffsetY: 5f, frontLegLift: 0f, backLegLift: 0f);

        private static readonly PlayerBodyPose Impact = new PlayerBodyPose(
            torsoAngle: -9f, headAngle: -4f, armAngle: -58f, shieldAngle: 3.5f,
            frontLegAngle: -2f, backLegAngle: 2f, capeAngle: 12f,
            hipOffsetX: 17f, hipOffsetY: 6f, frontLegLift: 0f, backLegLift: 0f);

        /// <summary>
        /// Samples the additive attack pose for a phase at normalized progress
        /// 0..1. Returns the neutral pose for Idle so blending back is smooth.
        /// </summary>
        public static PlayerBodyPose Sample(AttackPhase phase, float progress)
        {
            var t = progress < 0f ? 0f : progress > 1f ? 1f : progress;
            switch (phase)
            {
                case AttackPhase.Windup:
                    return PlayerBodyPose.Blend(PlayerBodyPose.Neutral, Windup,
                        EaseOut(t));
                case AttackPhase.Strike:
                    return PlayerBodyPose.Blend(Windup, Strike, EaseIn(t));
                case AttackPhase.Impact:
                    return PlayerBodyPose.Blend(Strike, Impact, t);
                case AttackPhase.Recovery:
                    return PlayerBodyPose.Blend(Impact, PlayerBodyPose.Neutral,
                        EaseOut(t));
                default:
                    return PlayerBodyPose.Neutral;
            }
        }

        private static float EaseOut(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        private static float EaseIn(float t)
        {
            return t * t;
        }
    }
}
