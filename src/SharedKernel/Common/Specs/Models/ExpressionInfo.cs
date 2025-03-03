using System.Linq.Expressions;

namespace SharedKernel.Common.Specs.Models;

public class ExpressionInfo
{
    public LambdaExpression LamdaExpression { get; set; } = default!;

    public Type? PropertyType { get; set; }

    public Type? EntityType { get; set; }
}
