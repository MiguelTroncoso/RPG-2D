namespace Lumbre.Game.Domain
{
    public sealed class SessionState
    {
        public SessionState(SessionStatus status, long revision)
        {
            Status = status;
            Revision = revision;
        }

        public SessionStatus Status { get; }
        public long Revision { get; }
    }
}
