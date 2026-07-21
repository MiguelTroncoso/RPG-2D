using System;
using Lumbre.Game.Domain;

namespace Lumbre.Game.Application
{
    public interface IGameSession
    {
        SessionStatus Status { get; }
        SessionState CurrentState { get; }
        event Action<GameEvent> EventPublished;

        void Start();
        void Stop();
        CommandResult Send(GameCommand command);
    }
}
