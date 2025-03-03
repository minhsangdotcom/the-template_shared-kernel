using SharedKernel.Common.Specs.Interfaces;

namespace SharedKernel.Common.Specs;

public class SpecificationBuilder<T>(Specification<T>? Spec) : ISpecificationBuilder<T>
    where T : class
{
    public Specification<T>? Spec { get; } = Spec;
}

public class IncludableSpecificationBuilder<T, TProperty>(Specification<T> Spec)
    : IIncludableSpecificationBuilder<T, TProperty>
    where T : class
{
    public Specification<T>? Spec { get; } = Spec;
}
