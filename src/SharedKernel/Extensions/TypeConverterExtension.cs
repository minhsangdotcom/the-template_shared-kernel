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

                if (input is DateTimeOffset dateTimeOffset)
                {
                    return DateOnly.FromDateTime(dateTimeOffset.UtcDateTime);
                }

                if (input is string date)
                {
                    if (DateTimeOffset.TryParse(date, out var dto))
                    {
                        return DateOnly.FromDateTime(dto.UtcDateTime);
                    }

                    if (DateTime.TryParse(date, out var datetime))
                    {
                        return DateOnly.FromDateTime(datetime);
                    }
                }
            }

            if (underlyingTargetType == typeof(DateTimeOffset))
            {
                if (input is DateTime dt2)
                {
                    return new DateTimeOffset(dt2, TimeZoneInfo.Local.GetUtcOffset(dt2));
                }

                if (input is DateOnly dateOnly)
                {
                    DateTime dateTime = dateOnly.ToDateTime(TimeOnly.MinValue);
                    return new DateTimeOffset(dateTime, TimeZoneInfo.Local.GetUtcOffset(dateTime));
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
