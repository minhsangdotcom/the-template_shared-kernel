using System.Collections;
using SharedKernel.Extensions.Reflections;

namespace SharedKernel.Extensions;

public static class TypeConverterExtension
{
    public static object? ConvertTo(this object? input, Type targetType)
    {
        if (input is null)
        {
            return null;
        }

        Type underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        Type inputType = input.GetType();
        if (
            targetType.IsAssignableFrom(inputType)
            || inputType.IsUserDefineType()
            || inputType.IsArrayGenericType()
            || (inputType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(inputType))
        )
        {
            return input;
        }

        try
        {
            if (underlyingTargetType == typeof(DateOnly))
            {
                if (input is DateTime dt)
                {
                    return DateOnly.FromDateTime(dt);
                }

                if (input is string date && DateOnly.TryParse(date, out DateOnly parsedDate))
                {
                    return parsedDate;
                }
            }

            if (underlyingTargetType == typeof(DateTimeOffset))
            {
                if (input is DateTime dt2)
                {
                    return new DateTimeOffset(dt2);
                }
                if (input is string date && DateTimeOffset.TryParse(date, out DateTimeOffset dto))
                {
                    return dto;
                }
            }

            return Convert.ChangeType(input, underlyingTargetType);
        }
        catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
        {
            throw new InvalidCastException($"Cannot convert from {inputType} to {targetType}", ex);
        }
    }
}
