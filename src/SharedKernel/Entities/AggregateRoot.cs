using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SharedKernel.DomainEvents;

namespace SharedKernel.Entities;

public abstract class AggregateRoot : AggregateRoot<Ulid>
{
    public override Ulid Id { get; protected set; } = Ulid.NewUlid();
}

public abstract class AggregateRoot<T> : Entity<T>, IAuditable
{
    public long Version { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonIgnore]
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> UncommittedEvents => uncommittedEvents;

    [JsonIgnore]
    [NotMapped]
    private readonly Queue<IDomainEvent> uncommittedEvents = [];

    public IDomainEvent[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Emit(IDomainEvent domainEvent)
    {
        if (TryApplyDomainEvent(domainEvent))
        {
            uncommittedEvents.Enqueue(domainEvent);
        }
    }

    protected abstract bool TryApplyDomainEvent(IDomainEvent domainEvent);
}
