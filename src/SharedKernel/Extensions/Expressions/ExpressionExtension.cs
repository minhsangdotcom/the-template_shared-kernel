using System.Linq.Expressions;
using System.Reflection;
using SharedKernel.Extensions.Reflections;

namespace SharedKernel.Extensions.Expressions;

public static class ExpressionExtension
{
    public static Expression MemberExpression<T>(
        this Expression expression,
        string propertyPath,
        bool isNullCheck = false
    )
    {
        Type type = typeof(T);
        string[] properties = propertyPath.Trim().Split('.', StringSplitOptions.TrimEntries);

        Expression propertyValue = expression;
        Expression nullCheck = null!;

        foreach (string property in properties)
        {
            PropertyInfo propertyInfo = type.GetNestedPropertyInfo(property);

            try
            {
                propertyValue = Expression.PropertyOrField(propertyValue, property);
            }
            catch (ArgumentException)
            {
                propertyValue = Expression.MakeMemberAccess(propertyValue, propertyInfo);
            }

            if (isNullCheck)
            {
                nullCheck = GenerateOrderNullCheckExpression(propertyValue, nullCheck);
            }

            type = propertyInfo.PropertyType;
        }

        return nullCheck == null
            ? propertyValue
            : Expression.Condition(
                nullCheck,
                Expression.Default(propertyValue.Type),
                propertyValue
            );
    }

    public static Expression MemberExpression(
        this Expression expression,
        Type entityType,
        string propertyPath
    )
    {
        Type type = entityType;
        string[] properties = propertyPath.Trim().Split('.', StringSplitOptions.TrimEntries);

        Expression propertyValue = expression;
        foreach (string property in properties)
        {
            PropertyInfo propertyInfo = type.GetNestedPropertyInfo(property);

            try
            {
                propertyValue = Expression.PropertyOrField(propertyValue, property);
            }
            catch (ArgumentException)
            {
                propertyValue = Expression.MakeMemberAccess(propertyValue, propertyInfo);
            }

            type = propertyInfo.PropertyType;
        }

        return propertyValue;
    }

    public static MemberExpressionResult MemberExpressionNullCheck(
        this Expression expression,
        Type entityType,
        string propertyPath
    )
    {
        Type type = entityType;
        string[] properties = propertyPath.Trim().Split('.', StringSplitOptions.TrimEntries);

        Expression propertyValue = expression;
        Expression nullCheck = null!;

        foreach (string property in properties)
        {
            PropertyInfo propertyInfo = type.GetNestedPropertyInfo(property);

            try
            {
                propertyValue = Expression.PropertyOrField(propertyValue, property);
            }
            catch (ArgumentException)
            {
                propertyValue = Expression.MakeMemberAccess(propertyValue, propertyInfo);
            }

            nullCheck = GenerateNullCheckExpression(propertyValue, nullCheck);

            type = propertyInfo.PropertyType;
        }

        return new(nullCheck, propertyValue);
    }

    public static PropertyInfo ToPropertyInfo(this Expression expression)
    {
        if (expression == null)
        {
            throw new ArgumentException("Expression must be not null.");
        }

        if (expression is not LambdaExpression lamda)
        {
            throw new ArgumentException($"Can not parse {expression} to LambdaExpression");
        }

        LambdaExpression lambdaExpression = lamda;

        ExpressionType expressionType = lambdaExpression.Body.NodeType;

        MemberExpression? memberExpr = expressionType switch
        {
            ExpressionType.Convert => ((UnaryExpression)lambdaExpression.Body).Operand
                as MemberExpression,
            ExpressionType.MemberAccess => lambdaExpression.Body as MemberExpression,
            _ => throw new Exception("Expression Type is not support"),
        };

        return (PropertyInfo)memberExpr!.Member;
    }

    public static string ToStringProperty(this Expression expression)
    {
        if (expression == null)
        {
            throw new ArgumentException("Expression must be not null.");
        }

        if (expression is not LambdaExpression lamda)
        {
            throw new ArgumentException($"Can not parse {expression} to LambdaExpression");
        }

        LambdaExpression lambdaExpression = lamda;

        var stack = new Stack<string>();

        ExpressionType expressionType = lambdaExpression.Body.NodeType;

        MemberExpression? memberExpression = true switch
        {
            bool
                when expressionType == ExpressionType.Convert
                    || expressionType == ExpressionType.ConvertChecked => (
                lambdaExpression.Body as UnaryExpression
            )?.Operand as MemberExpression,
            bool when expressionType == ExpressionType.MemberAccess => lambdaExpression.Body
                as MemberExpression,
            _ => throw new Exception("Expression Type is not support"),
        };

        while (memberExpression != null)
        {
            stack.Push(memberExpression.Member.Name);
            memberExpression = memberExpression.Expression as MemberExpression;
        }

        return string.Join(".", [.. stack]);
    }

    public static Type GetMemberExpressionType(this MemberExpression expression)
    {
        if (expression == null)
        {
            throw new ArgumentException("Expression must be not null.");
        }

        if (expression is not MemberExpression memberExpression)
        {
            throw new ArgumentException($"Can not parse {expression} to MemberExpression");
        }

        return memberExpression.Member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new ArgumentException(
                $"{memberExpression.Member} neither a property nor a field "
            ),
        };
    }

    private static BinaryExpression GenerateOrderNullCheckExpression(
        Expression propertyValue,
        Expression nullCheckExpression
    ) =>
        nullCheckExpression == null
            ? Expression.Equal(propertyValue, Expression.Default(propertyValue.Type))
            : Expression.OrElse(
                nullCheckExpression,
                Expression.Equal(propertyValue, Expression.Default(propertyValue.Type))
            );

    private static Expression GenerateNullCheckExpression(
        Expression propertyValue,
        Expression nullCheckExpression
    )
    {
        Expression notExpression = Expression.Not(
            Expression.Equal(propertyValue, Expression.Constant(null, propertyValue.Type))
        );
        return nullCheckExpression == null
            ? notExpression
            : Expression.AndAlso(nullCheckExpression, notExpression);
    }
}

public record MemberExpressionResult(Expression NullCheck, Expression Member);
