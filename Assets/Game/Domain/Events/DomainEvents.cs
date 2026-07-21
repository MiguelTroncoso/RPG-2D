using System;

namespace Lumbre.Game.Domain.Events
{
    public interface IDomainEvent
    {
    }

    public sealed class DomainEventBus
    {
        public event Action<IDomainEvent> Published;

        public void Publish(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            Published?.Invoke(domainEvent);
        }
    }

    public enum CombatantKind
    {
        Mordeluz = 0,
        MordeluzResonante = 1
    }

    public readonly struct CombatantDefeatedEvent : IDomainEvent
    {
        public CombatantDefeatedEvent(CombatantKind kind, string entityId)
        {
            Kind = kind;
            EntityId = entityId ?? string.Empty;
        }

        public CombatantKind Kind { get; }
        public string EntityId { get; }
    }
}
