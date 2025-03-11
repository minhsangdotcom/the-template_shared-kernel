using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using SharedKernel.Exceptions;

namespace SharedKernel.Extensions.Reflections;

public static class PropertyInfoExtensions
{
    public static bool IsArrayGenericType(this PropertyInfo propertyInfo) =>
        propertyInfo.PropertyType.IsArrayGenericType();

    public static bool IsArrayGenericType(this Type type)
    {
        if (
            type.IsGenericType
            && typeof(IEnumerable).IsAssignableFrom(type)
            && type.GetGenericArguments()[0].IsUserDefineType()
        )
        {
            return true;
        }
        return false;
    }

    public static PropertyInfo GetNestedPropertyInfo(this Type type, string propertyName)
    {
        // Split the propertyName by '.' to handle nested properties
        var propertyParts = propertyName.Trim().Split('.');

        PropertyInfo? propertyInfo = null;

        // Iterate through each part of the property chain
        foreach (var part in propertyParts)
        {
            // Attempt to find the property information for the current part
            propertyInfo =
                type.GetProperty(
                    part.Trim(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                ) ?? throw new NotFoundException(part, part);

            // Move to the next type in the chain (the type of the current property)
            type = propertyInfo.IsArrayGenericType()
                ? propertyInfo.PropertyType.GetGenericArguments()[0]
                : propertyInfo.PropertyType;
        }

        // Return the last found PropertyInfo (non-null due to Guard.Against)
        return propertyInfo!;
    }

    public static object? GetNestedPropertyValue(this Type type, string propertyName, object target)
    {
        var propertyParts = propertyName.Trim().Split('.');
        object? objTarget = target;

        foreach (var part in propertyParts)
        {
            if (objTarget == null)
            {
                break;
            }

            PropertyInfo propertyInfo =
                type.GetProperty(
                    part.Trim(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                ) ?? throw new NotFoundException(part, part);

            type = propertyInfo.IsArrayGenericType()
                ? propertyInfo.PropertyType.GetGenericArguments()[0]
                : propertyInfo.PropertyType;

            objTarget = propertyInfo.GetValue(objTarget, null);
        }

        return objTarget;
    }

    public static bool IsNestedPropertyValid(this Type type, string propertyName)
    {
        var propertyParts = propertyName.Trim().Split('.');

        foreach (var part in propertyParts)
        {
            PropertyInfo? propertyInfo = type.GetProperty(
                part.Trim(),
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
            );

            if (propertyInfo == null)
            {
                return false;
            }

            type = propertyInfo.IsArrayGenericType()
                ? propertyInfo.PropertyType.GetGenericArguments()[0]
                : propertyInfo.PropertyType;
        }

        return true;
    }

    public static bool IsUserDefineType(this PropertyInfo? propertyInfo) =>
        (propertyInfo?.PropertyType).IsUserDefineType();

    public static bool IsUserDefineType(this Type? type)
    {
        if (type == null)
        {
            return false;
        }

        return type?.IsClass == true && type?.FullName?.StartsWith("System.") == false;
    }

    public static string GetValue<T>(this T obj, Expression<Func<T, object>> expression)
    {
        PropertyInfo propertyInfo = expression.ToPropertyInfo();

        return propertyInfo.GetValue(obj, null)?.ToString() ?? string.Empty;
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

    public static bool IsNullable(this Type type)
    {
        if (!type.IsValueType)
            return true; // ref-type
        if (Nullable.GetUnderlyingType(type) != null)
            return true; // Nullable<T>
        return false; // value-type
    }
}
