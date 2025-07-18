using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Exceptions;
using SharedKernel.Extensions.Expressions;
using SharedKernel.Extensions.Reflections;
using SharedKernel.Results;

namespace SharedKernel.Extensions.QueryExtensions;

public static class FilterExtension
{
    public static IQueryable<T> Filter<T>(
        this IQueryable<T> query,
        object? filterObject,
        DbProvider? dbProvider = DbProvider.Postgresql
    )
    {
        if (filterObject == null)
        {
            return query;
        }

        Type type = typeof(T);
        ParameterExpression parameter = Expression.Parameter(type, "a");
        Expression expression = FilterExpression(filterObject, parameter, type, "b", dbProvider);

        var lamda = Expression.Lambda<Func<T, bool>>(expression, parameter);

        return query.Where(lamda);
    }

    public static IEnumerable<T> Filter<T>(this IEnumerable<T> query, object? filterObject) =>
        query.AsQueryable().Filter(filterObject);

    private static Expression FilterExpression(
        dynamic filterObject,
        Expression paramOrMember,
        Type type,
        string parameterName,
        DbProvider? dbProvider
    )
    {
        var dynamicFilters = (IDictionary<string, object>)filterObject;

        Expression body = null!;
        foreach (var dynamicFilter in dynamicFilters)
        {
            string propertyName = dynamicFilter.Key;
            object value = dynamicFilter.Value;

            int isAndOperator = string.Compare(
                propertyName,
                "$and",
                StringComparison.OrdinalIgnoreCase
            );
            int isOrOperator = string.Compare(
                propertyName,
                "$or",
                StringComparison.OrdinalIgnoreCase
            );

            if (isAndOperator != 0 && isOrOperator != 0 && propertyName.Contains('$'))
            {
                Expression left = paramOrMember;
                return Compare(propertyName, left, value, dbProvider);
            }

            Expression expression = null!;
            if (value is IEnumerable<object> values)
            {
                expression = ProcessList(
                    values,
                    new(paramOrMember, type, parameterName.NextUniformSequence()),
                    isAndOperator == 0 && isOrOperator != 0
                );
            }
            else
            {
                PropertyInfo propertyInfo = type.GetNestedPropertyInfo(propertyName);
                Type propertyType = propertyInfo.PropertyType;

                Expression memberExpression = paramOrMember.MemberExpression(type, propertyName);

                expression = ProcessObject(
                    propertyInfo,
                    new(memberExpression, propertyType, parameterName.NextUniformSequence(), value),
                    dbProvider
                );
            }

            body = body == null ? expression : Expression.AndAlso(body, expression);
        }

        return body;
    }

    /// <summary>
    /// Process array object property in filter
    /// </summary>
    /// <param name="values"></param>
    /// <param name="payload"></param>
    /// <param name="isAndOperator"></param>
    /// <returns></returns>
    private static Expression ProcessList(
        IEnumerable<object> values,
        FilterExpressionPayload payload,
        bool isAndOperator = false,
        DbProvider? dbProvider = DbProvider.Postgresql
    )
    {
        Expression body = null!;
        foreach (var value in values)
        {
            Expression expression = FilterExpression(
                value,
                payload.ParamOrMember,
                payload.Type,
                payload.ParameterName,
                dbProvider
            );

            body =
                body == null ? expression
                : isAndOperator ? Expression.AndAlso(body, expression)
                : Expression.OrElse(body, expression);
        }

        return body;
    }

    /// <summary>
    /// Process object property in filter
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    private static Expression ProcessObject(
        PropertyInfo propertyInfo,
        FilterExpressionPayload payload,
        DbProvider? dbProvider = DbProvider.Postgresql
    )
    {
        Type propertyType = payload.Type;

        //current property is array object then generate nested any
        if (propertyType.IsArrayGenericType())
        {
            propertyType = propertyInfo.PropertyType.GetGenericArguments()[0];
            ParameterExpression anyParameter = Expression.Parameter(
                propertyType,
                payload.ParameterName
            );

            Expression operationBody = FilterExpression(
                payload.Value!,
                anyParameter,
                propertyType,
                payload.ParameterName,
                dbProvider
            );

            LambdaExpression anyLamda = Expression.Lambda(operationBody, anyParameter);

            return Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Any),
                [propertyType],
                payload.ParamOrMember,
                anyLamda
            );
        }

        return FilterExpression(
            payload.Value!,
            payload.ParamOrMember,
            propertyType,
            payload.ParameterName,
            dbProvider
        );
    }

    /// <summary>
    /// Do comparison
    /// </summary>
    /// <param name="operationString"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    private static BinaryExpression Compare(
        string operationString,
        Expression left,
        object right,
        DbProvider? dbProvider = DbProvider.Postgresql
    )
    {
        OperationType operationType = GetOperationType(operationString);

        if (operationType == OperationType.Between)
        {
            return CompareBetweenOperations(left, right);
        }

        bool isFound = BinaryComparisons.TryGetValue(operationType, out var comparisonFunc);
        if (!isFound)
        {
            if (!MethodCallComparisons.TryGetValue(operationType, out var callMethodType))
            {
                throw new NotFoundException(nameof(operationType), nameof(operationType));
            }

            return CompareMethodCallOperations(left, right, callMethodType, operationType);
        }

        return CompareBinaryOperations(left, right, comparisonFunc!, operationType, dbProvider);
    }

    private static BinaryExpression CompareBetweenOperations(Expression left, object right)
    {
        ConvertObjectTypeResult convertObjectTypeResult = ParseArray((MemberExpression)left, right);

        List<Func<Expression, Expression, BinaryExpression>> operations =
        [
            BinaryComparisons[OperationType.Gte],
            BinaryComparisons[OperationType.Lte],
        ];

        BinaryExpression body = null!;
        IList rightValues = (IList)convertObjectTypeResult.Value!;
        for (int i = 0; i < operations.Count; i++)
        {
            var operation = operations[i];
            var rightValue = rightValues[i];

            BinaryExpression expression = operation(
                convertObjectTypeResult.Member,
                Expression.Constant(rightValue, convertObjectTypeResult.Type)
            );

            body = body == null ? expression : Expression.AndAlso(body, expression);
        }

        return body;
    }

    private static BinaryExpression CompareBinaryOperations(
        Expression left,
        object right,
        Func<Expression, Expression, BinaryExpression> comparisonFunc,
        OperationType operationType,
        DbProvider? dbProvider = DbProvider.Postgresql
    )
    {
        if (
            operationType.ToString().EndsWith("i", StringComparison.OrdinalIgnoreCase)
            && ((MemberExpression)left).GetMemberExpressionType() == typeof(string)
        )
        {
            if (dbProvider == DbProvider.Postgresql)
            {
                return EqualCaseInSensitive(left, right);
            }

            MethodCallExpression member = Expression.Call(
                left,
                nameof(string.ToLower),
                Type.EmptyTypes
            );
            MethodCallExpression value = Expression.Call(
                Expression.Constant(right),
                nameof(string.ToLower),
                Type.EmptyTypes
            );

            return comparisonFunc(member, value);
        }
        ConvertExpressionTypeResult convert = ParseObject((MemberExpression)left, right);
        return comparisonFunc(convert.Member, convert.Value);
    }

    private static BinaryExpression CompareMethodCallOperations(
        Expression left,
        object right,
        KeyValuePair<Type, string> callMethodType,
        OperationType operationType,
        DbProvider? dbProvider = DbProvider.Postgresql
    )
    {
        if (operationType == OperationType.In || operationType == OperationType.NotIn)
        {
            ConvertObjectTypeResult convertObjectTypeResult = ParseArray(
                (MemberExpression)left,
                right
            );

            MethodInfo methodInfo = callMethodType
                .Key.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == callMethodType.Value && m.GetParameters().Length == 2)
                .MakeGenericMethod(convertObjectTypeResult.Type);

            Expression expression = Expression.Call(
                methodInfo,
                convertObjectTypeResult.Constant,
                convertObjectTypeResult.Member
            );

            return NotOr(operationType, expression);
        }

        Expression outer = left;
        Expression inner = Expression.Constant(right.ToString());
        if (operationType.ToString().EndsWith("i", StringComparison.OrdinalIgnoreCase))
        {
            if (dbProvider == DbProvider.Postgresql)
            {
                return CompareCaseInSensitive(left, right, operationType);
            }

            outer = Expression.Call(left, nameof(string.ToLower), Type.EmptyTypes);
            inner = Expression.Call(
                Expression.Constant(right.ToString()),
                nameof(string.ToLower),
                Type.EmptyTypes
            );
        }

        MethodCallExpression result = Expression.Call(
            outer,
            callMethodType.Key.GetMethod(callMethodType.Value, [typeof(string)])!,
            inner
        );

        return NotOr(operationType, result);
    }

    /// <summary>
    /// just for postgresql
    /// </summary>
    /// <param name="left"></param>
    /// <param name="value"></param>
    /// <param name="operationType"></param>
    /// <returns></returns>
    private static BinaryExpression CompareCaseInSensitive(
        Expression left,
        object? value,
        OperationType operationType
    )
    {
        MethodInfo iLikeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
            nameof(NpgsqlDbFunctionsExtensions.ILike),
            [typeof(DbFunctions), typeof(string), typeof(string)]
        )!;

        MemberExpression efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        MethodInfo unaccentMethod = typeof(PostgresDbFunctionExtensions).GetMethod(
            nameof(PostgresDbFunctionExtensions.Unaccent),
            [typeof(string)]
        )!;

        MethodCallExpression leftExpression = Expression.Call(unaccentMethod, left);
        MethodCallExpression rightExpression = Expression.Call(
            unaccentMethod,
            Expression.Constant($"%{value}%")
        );

        var iLikeMethodCall = Expression.Call(
            iLikeMethod,
            efFunctions,
            leftExpression,
            rightExpression
        );
        return NotOr(operationType, iLikeMethodCall);
    }

    private static BinaryExpression EqualCaseInSensitive(Expression left, object? value)
    {
        MemberExpression efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        MethodInfo unaccentMethod = typeof(PostgresDbFunctionExtensions).GetMethod(
            nameof(PostgresDbFunctionExtensions.Unaccent),
            [typeof(string)]
        )!;

        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        MethodCallExpression leftToLower = Expression.Call(left, toLowerMethod);
        string? rightValue = value?.ToString()?.ToLower();

        MethodCallExpression leftExpression = Expression.Call(unaccentMethod, leftToLower);
        MethodCallExpression rightExpression = Expression.Call(
            unaccentMethod,
            Expression.Constant(rightValue)
        );

        return Expression.Equal(leftExpression, rightExpression);
    }

    private static BinaryExpression NotOr(OperationType operationType, Expression expression) =>
        operationType.ToString().StartsWith("not", StringComparison.OrdinalIgnoreCase)
            ? Expression.Equal(expression, Expression.Constant(false))
            : Expression.Equal(expression, Expression.Constant(true));

    /// <summary>
    /// Change both types to the same type
    /// </summary>
    /// <param name="memberExpression"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static ConvertExpressionTypeResult ParseObject(MemberExpression left, object right)
    {
        ConvertObjectTypeResult parse = Parse(left, right);
        return new(parse.Member, parse.Constant);
    }

    /// <summary>
    /// Change both types to the same type for array value in $in and $notIn
    /// </summary>
    /// <param name="memberExpression"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static ConvertObjectTypeResult ParseArray(MemberExpression left, object right)
    {
        IList rightValues = (IList)right;
        int count = rightValues.Count;

        List<object?> results = [];
        Expression member = null!;
        Type type = null!;
        for (int i = 0; i < count; i++)
        {
            object? rightValue = rightValues[i];
            var result = Parse(left, rightValue!);

            if (member == null && type == null)
            {
                member = result.Member;
                type = result.Type;
            }

            results.Add(result.Value);
        }

        (IList list, Type convertedType) = ConvertListToType(results!, type);
        return new(member!, list, Expression.Constant(list, convertedType), type);
    }

    /// <summary>
    /// Convert list object to explicit type
    /// </summary>
    /// <param name="sourceList"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    static (IList, Type) ConvertListToType(List<object> sourceList, Type targetType)
    {
        // Step 1: Create a generic List<T> where T is the targetType
        Type listType = typeof(List<>).MakeGenericType(targetType);
        IList typedList = (IList)Activator.CreateInstance(listType)!;

        // Step 2: Iterate through the source list and convert each item to the targetType
        foreach (var item in sourceList)
        {
            // Step 3: Add each item to the typed list (casting to the target type)
            Type type = targetType;

            if (targetType.IsNullable() && targetType.GenericTypeArguments.Length > 0)
            {
                type = targetType.GenericTypeArguments[0];
            }
            //typedList.Add(Convert.ChangeType(item, type));
            typedList.Add(item.ConvertTo(type));
        }

        return new(typedList, listType);
    }

    private static ConvertObjectTypeResult Parse(MemberExpression left, object? right)
    {
        Expression member = left;
        Type leftType = left.GetMemberExpressionType();
        Type? rightType = right?.GetType();

        if (leftType != rightType)
        {
            Type targetType = leftType;

            if (targetType.IsNullable() && targetType.GenericTypeArguments.Length > 0)
            {
                targetType = targetType.GenericTypeArguments[0];
            }
            object? changedTypeValue = right.ConvertTo(targetType);
            return new(
                member,
                changedTypeValue,
                Expression.Constant(changedTypeValue, leftType),
                leftType
            );
        }

        return new(member, right, Expression.Constant(right), leftType);
    }

    /// <summary>
    /// Convert string operation to enum
    /// </summary>
    /// <param name="operationString"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static OperationType GetOperationType(string operationString)
    {
        // Extract the operation substring (remove the first character, e.g., '$')
        string operation = operationString[1..];

        // Try to parse the enum, handling case-insensitive matching
        if (
            Enum.TryParse(typeof(OperationType), operation, true, out var result)
            && result is OperationType parsedOperation
        )
        {
            return parsedOperation;
        }

        // Handle the case where no valid enum was found (optional, you can throw or return default)
        throw new ArgumentException($"Invalid operation: {operationString}");
    }

    private static readonly Dictionary<
        OperationType,
        Func<Expression, Expression, BinaryExpression>
    > BinaryComparisons =
        new()
        {
            { OperationType.Eq, Expression.Equal },
            { OperationType.EqI, Expression.Equal },
            { OperationType.Ne, Expression.NotEqual },
            { OperationType.NeI, Expression.NotEqual },
            { OperationType.Lt, Expression.LessThan },
            { OperationType.Lte, Expression.LessThanOrEqual },
            { OperationType.Gt, Expression.GreaterThan },
            { OperationType.Gte, Expression.GreaterThanOrEqual },
        };

    private static readonly Dictionary<
        OperationType,
        KeyValuePair<Type, string>
    > MethodCallComparisons =
        new()
        {
            { OperationType.In, new(typeof(Enumerable), nameof(Enumerable.Contains)) },
            { OperationType.NotIn, new(typeof(Enumerable), nameof(Enumerable.Contains)) },
            { OperationType.Contains, new(typeof(string), nameof(string.Contains)) },
            { OperationType.ContainsI, new(typeof(string), nameof(string.Contains)) },
            { OperationType.NotContains, new(typeof(string), nameof(string.Contains)) },
            { OperationType.NotContainsI, new(typeof(string), nameof(string.Contains)) },
            { OperationType.StartsWith, new(typeof(string), nameof(string.StartsWith)) },
            { OperationType.EndsWith, new(typeof(string), nameof(string.EndsWith)) },
        };
}

internal record ConvertObjectTypeResult(
    Expression Member,
    object? Value,
    ConstantExpression Constant,
    Type Type
);

internal record FilterExpressionPayload(
    Expression ParamOrMember,
    Type Type,
    string ParameterName,
    object? Value = null
);

internal enum OperationType
{
    Eq = 1,
    EqI = 2,
    Ne = 3,
    NeI = 4,
    In = 5,
    NotIn = 6,
    Lt = 7,
    Lte = 8,
    Gt = 9,
    Gte = 10,
    Between = 11,
    NotContains = 12,
    NotContainsI = 13,
    Contains = 14,
    ContainsI = 15,
    StartsWith = 16,
    EndsWith = 17,
}
