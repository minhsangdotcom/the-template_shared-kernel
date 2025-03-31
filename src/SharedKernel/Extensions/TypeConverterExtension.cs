using System.Collections;
using SharedKernel.Extensions.Reflections;

namespace SharedKernel.Extensions;

public static class TypeConverterExtension
{
    public static object? ConvertTo(this object? input, Type targetType)
    {
        if (input == null)
        {
            return null;
        }

        Type inputType = input.GetType();
        if (
            targetType.IsAssignableFrom(input.GetType())
            || inputType.IsUserDefineType()
            || inputType.IsArrayGenericType()
            || (inputType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(inputType))
        )
        {
            return input;
        }

        try
        {
            if (targetType == typeof(DateTimeOffset))
            {
                if (input is DateTime dt)
                {
                    return new DateTimeOffset(dt);
                }
                if (DateTimeOffset.TryParse(input.ToString(), out DateTimeOffset dto))
                {
                    return dto;
                }
            }

            return Convert.ChangeType(input, targetType);
        }
        catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
        {
            throw new InvalidCastException($"Cannot convert {input.GetType()} to {targetType}", ex);
        }
    }
}
