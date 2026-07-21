namespace Lumbre.Game.Application
{
    public sealed class CommandResult
    {
        private CommandResult(bool succeeded, string errorCode)
        {
            Succeeded = succeeded;
            ErrorCode = errorCode;
        }

        public bool Succeeded { get; }
        public string ErrorCode { get; }

        public static CommandResult Accepted()
        {
            return new CommandResult(true, string.Empty);
        }

        public static CommandResult Rejected(string errorCode)
        {
            return new CommandResult(false, errorCode);
        }
    }
}
