using System;
using Lumbre.Game.Application;
using Lumbre.Game.Domain;

namespace Lumbre.Game.Infrastructure.Local
{
    public sealed class LocalGameSession : IGameSession
    {
        private long _sequence;

        public LocalGameSession()
        {
            Status = SessionStatus.NotStarted;
            CurrentState = new SessionState(Status, 0);
        }

        public SessionStatus Status { get; private set; }
        public SessionState CurrentState { get; private set; }
        public event Action<GameEvent> EventPublished;

        public void Start()
        {
            if (Status == SessionStatus.Running)
            {
                return;
            }

            Status = SessionStatus.Running;
            CurrentState = new SessionState(Status, CurrentState.Revision + 1);
            Publish("SessionStarted");
        }

        public void Stop()
        {
            if (Status == SessionStatus.Stopped)
            {
                return;
            }

            Status = SessionStatus.Stopped;
            CurrentState = new SessionState(Status, CurrentState.Revision + 1);
            Publish("SessionStopped");
        }

        public CommandResult Send(GameCommand command)
        {
            if (command == null)
            {
                return CommandResult.Rejected("Command.Null");
            }

            if (Status != SessionStatus.Running)
            {
                return CommandResult.Rejected("Session.NotRunning");
            }

            return CommandResult.Rejected("Command.NotImplementedInH1");
        }

        private void Publish(string typeId)
        {
            _sequence++;
            EventPublished?.Invoke(new GameEvent(typeId, _sequence));
        }
    }
}
