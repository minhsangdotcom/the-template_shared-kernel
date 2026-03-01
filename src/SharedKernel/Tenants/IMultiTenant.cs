namespace SharedKernel.Tenants;

public interface IMultiTenant<T>
{
    public T? TenantId { get; }
}
