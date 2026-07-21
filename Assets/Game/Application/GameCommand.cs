namespace Lumbre.Game.Application
{
    public abstract class GameCommand
    {
        protected GameCommand(string typeId)
        {
            TypeId = typeId;
        }

        public string TypeId { get; }
    }
}
