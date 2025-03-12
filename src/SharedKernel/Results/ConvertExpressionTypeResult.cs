using System.Linq.Expressions;

namespace SharedKernel.Results;

public record ConvertExpressionTypeResult(Expression Member, ConstantExpression Value);
