using System.Linq.Expressions;

namespace SharedKernel.Results;

internal record ConvertExpressionTypeResult(Expression Member, ConstantExpression Value);
