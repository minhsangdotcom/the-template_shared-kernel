namespace SharedKernel.Entities;

public abstract class Entity<T>
{
    public virtual T Id { get; protected set; } = default!;

    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
}

public abstract class AuditableEntity<T> : Entity<T>, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public interface IAuditable : IBaseAuditable;
