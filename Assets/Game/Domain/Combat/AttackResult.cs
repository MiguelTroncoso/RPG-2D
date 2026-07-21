namespace Lumbre.Game.Domain.Combat
{
    public enum AttackResultCode
    {
        Success = 0,
        InvalidTarget = 1,
        TargetDead = 2,
        Cooldown = 3
    }

    public readonly struct AttackResult
    {
        private AttackResult(AttackResultCode code, DamageResult damage)
        {
            Code = code;
            Damage = damage;
        }

        public AttackResultCode Code { get; }
        public DamageResult Damage { get; }
        public bool Succeeded => Code == AttackResultCode.Success;

        public static AttackResult Success(DamageResult damage)
        {
            return new AttackResult(AttackResultCode.Success, damage);
        }

        public static AttackResult Failure(AttackResultCode code)
        {
            return new AttackResult(code, default);
        }
    }
}
