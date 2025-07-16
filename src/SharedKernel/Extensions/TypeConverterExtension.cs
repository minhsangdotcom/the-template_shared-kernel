using System.Collections;
using deniszykov.TypeConversion;
using SharedKernel.Extensions.Reflections;

namespace SharedKernel.Extensions;

public static class TypeConverterExtension
{
    /// <summary>
    /// convert only string object to specific type
    /// </summary>
    /// <param name="input"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public static object? ConvertTo(this object? input, Type targetType)
    {
        if (input is null)
        {
            return null;
        }

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

        Type underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (underlyingTargetType == typeof(DateTime))
        {
            return Convert.ChangeType(input, underlyingTargetType);
        }

        if (underlyingTargetType == typeof(Ulid))
        {
            Ulid ulid = input == null ? Ulid.Empty : Ulid.Parse(input.ToString());
            return ulid;
        }

        var conversionProvider = new TypeConversionProvider();
        object? result = conversionProvider.Convert(typeof(object), targetType, input);
        return result;
    }
}
