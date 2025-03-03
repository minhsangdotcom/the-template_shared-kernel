using System.Linq.Expressions;
using SharedKernel.Common.Specs.Models;

namespace SharedKernel.Common.Specs.Interfaces;

public interface ISpecification<T>
    where T : class
{
    SpecificationBuilder<T> Query { get; }

    Expression<Func<T, bool>> Criteria { get; }

    List<IncludeInfo> Includes { get; }

    bool IsNoTracking { get; }

    bool IsSplitQuery { get; }

    bool CacheEnabled { get; }

    string? CacheKey { get; }
}
