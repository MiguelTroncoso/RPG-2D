namespace Lumbre.Game.Application
{
    public sealed class GameEvent
    {
        public GameEvent(string typeId, long sequence)
        {
            TypeId = typeId;
            Sequence = sequence;
        }

        public string TypeId { get; }
        public long Sequence { get; }
    }
}
