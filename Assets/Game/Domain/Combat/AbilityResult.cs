namespace Lumbre.Game.Domain.Combat
{
    public enum AbilityResultCode
    {
        Success = 0,
        Cooldown = 1,
        InsufficientHeat = 2,
        AlreadyActive = 3,
        NotReady = 4,
        InvalidState = 5
    }

    public readonly struct AbilityResult
    {
        private AbilityResult(AbilityResultCode code)
        {
            Code = code;
        }

        public AbilityResultCode Code { get; }
        public bool Succeeded => Code == AbilityResultCode.Success;

        public static AbilityResult Success()
        {
            return new AbilityResult(AbilityResultCode.Success);
        }

        public static AbilityResult Failure(AbilityResultCode code)
        {
            return new AbilityResult(code);
        }
    }
}
