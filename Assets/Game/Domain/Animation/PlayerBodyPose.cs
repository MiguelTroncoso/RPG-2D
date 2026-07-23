namespace Lumbre.Game.Domain.Animation
{
    /// <summary>
    /// One sampled skeleton pose for the cutout player rig. Angles are in
    /// degrees (positive = counter-clockwise on screen) and offsets are in
    /// source-illustration pixels so the client can divide by pixels-per-unit.
    /// </summary>
    public readonly struct PlayerBodyPose
    {
        public PlayerBodyPose(float torsoAngle, float headAngle, float armAngle,
            float shieldAngle, float frontLegAngle, float backLegAngle,
            float capeAngle, float hipOffsetX, float hipOffsetY,
            float frontLegLift, float backLegLift)
        {
            TorsoAngle = torsoAngle;
            HeadAngle = headAngle;
            ArmAngle = armAngle;
            ShieldAngle = shieldAngle;
            FrontLegAngle = frontLegAngle;
            BackLegAngle = backLegAngle;
            CapeAngle = capeAngle;
            HipOffsetX = hipOffsetX;
            HipOffsetY = hipOffsetY;
            FrontLegLift = frontLegLift;
            BackLegLift = backLegLift;
        }

        public float TorsoAngle { get; }
        public float HeadAngle { get; }
        public float ArmAngle { get; }
        public float ShieldAngle { get; }
        public float FrontLegAngle { get; }
        public float BackLegAngle { get; }
        public float CapeAngle { get; }
        public float HipOffsetX { get; }
        public float HipOffsetY { get; }
        public float FrontLegLift { get; }
        public float BackLegLift { get; }

        public static PlayerBodyPose Neutral => new PlayerBodyPose(
            0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        public static PlayerBodyPose Blend(PlayerBodyPose from, PlayerBodyPose to,
            float weight)
        {
            var t = weight < 0f ? 0f : weight > 1f ? 1f : weight;
            return new PlayerBodyPose(
                Lerp(from.TorsoAngle, to.TorsoAngle, t),
                Lerp(from.HeadAngle, to.HeadAngle, t),
                Lerp(from.ArmAngle, to.ArmAngle, t),
                Lerp(from.ShieldAngle, to.ShieldAngle, t),
                Lerp(from.FrontLegAngle, to.FrontLegAngle, t),
                Lerp(from.BackLegAngle, to.BackLegAngle, t),
                Lerp(from.CapeAngle, to.CapeAngle, t),
                Lerp(from.HipOffsetX, to.HipOffsetX, t),
                Lerp(from.HipOffsetY, to.HipOffsetY, t),
                Lerp(from.FrontLegLift, to.FrontLegLift, t),
                Lerp(from.BackLegLift, to.BackLegLift, t));
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
