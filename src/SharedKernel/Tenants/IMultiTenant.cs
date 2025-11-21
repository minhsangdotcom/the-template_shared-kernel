namespace SharedKernel.Tenants;

public interface IMultiTenant : IMultiTenant<Ulid>;

public interface IMultiTenant<T>
{
    public T? TenantId { get; }
}
