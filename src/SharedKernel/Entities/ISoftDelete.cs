namespace SharedKernel.Entities;

public interface ISoftDelete
{
    public bool IsDeleted { get; }
}
