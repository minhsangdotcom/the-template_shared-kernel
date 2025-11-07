using System.Collections;
using System.Globalization;
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
        if (underlyingTargetType == typeof(Ulid))
        {
            Ulid ulid = input == null ? Ulid.Empty : Ulid.Parse(input.ToString());
            return ulid;
        }

        // DateOnly
        if (underlyingTargetType == typeof(DateOnly))
        {
            // string inputs
            if (input is string s)
            {
                s = s.Trim();

                if (DateOnly.TryParse(s, out DateOnly d1))
                {
                    return d1;
                }

                if (DateTimeOffset.TryParse(s, out DateTimeOffset dto))
                {
                    return DateOnly.FromDateTime(dto.Date);
                }

                if (
                    DateTime.TryParse(
                        s,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                        out DateTime dts
                    )
                )
                {
                    return DateOnly.FromDateTime(dts);
                }

                if (long.TryParse(s, out long unix))
                {
                    DateTimeOffset dto2 =
                        s.Length >= 13
                            ? DateTimeOffset.FromUnixTimeMilliseconds(unix)
                            : DateTimeOffset.FromUnixTimeSeconds(unix);
                    return DateOnly.FromDateTime(dto2.Date);
                }

                throw new InvalidCastException($"Cannot convert '{input}' to DateOnly.");
            }

            if (input is DateOnly d)
            {
                return d;
            }

            if (input is DateTime dt)
            {
                return DateOnly.FromDateTime(dt);
            }

            if (input is DateTimeOffset dtoInput)
            {
                return DateOnly.FromDateTime(dtoInput.Date);
            }

            if (input is long l)
            {
                DateTimeOffset dto =
                    l >= 1_000_000_000_000
                        ? DateTimeOffset.FromUnixTimeMilliseconds(l)
                        : DateTimeOffset.FromUnixTimeSeconds(l);

                return DateOnly.FromDateTime(dto.UtcDateTime);
            }

            if (input is int i)
            {
                var dto = DateTimeOffset.FromUnixTimeSeconds(i);
                return DateOnly.FromDateTime(dto.UtcDateTime);
            }

            throw new InvalidCastException($"Cannot convert '{input}' to DateOnly.");
        }

        if (underlyingTargetType == typeof(DateTime))
        {
            if (input is string s)
            {
                if (long.TryParse(s, out long unixLong))
                {
                    var dto2 =
                        s.Length >= 13
                            ? DateTimeOffset.FromUnixTimeMilliseconds(unixLong)
                            : DateTimeOffset.FromUnixTimeSeconds(unixLong);
                    return dto2.UtcDateTime;
                }
                return Convert.ChangeType(input, typeof(DateTime));
            }
            if (input is DateTime dtValue)
            {
                return dtValue;
            }

            if (input is DateTimeOffset dtoValue)
            {
                return dtoValue.UtcDateTime;
            }

            if (input is DateOnly dOnly)
            {
                return dOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
            }

            if (input is long l)
            {
                DateTimeOffset dto =
                    l >= 1_000_000_000_000
                        ? DateTimeOffset.FromUnixTimeMilliseconds(l)
                        : DateTimeOffset.FromUnixTimeSeconds(l);

                return dto.UtcDateTime;
            }

            if (input is int i)
            {
                var dto = DateTimeOffset.FromUnixTimeSeconds(i);
                return dto.UtcDateTime;
            }

            throw new InvalidCastException($"Cannot convert '{input.GetType()}' to DateTime.");
        }

        if (underlyingTargetType == typeof(DateTimeOffset))
        {
            if (input is string s)
            {
                if (long.TryParse(s, out long unixLong))
                {
                    var dto2 =
                        s.Length >= 13
                            ? DateTimeOffset.FromUnixTimeMilliseconds(unixLong)
                            : DateTimeOffset.FromUnixTimeSeconds(unixLong);
                    return dto2;
                }
            }
            if (input is DateOnly dOnly)
            {
                return new DateTimeOffset(dOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local));
            }

            if (input is long l)
            {
                var dto =
                    l >= 1_000_000_000_000
                        ? DateTimeOffset.FromUnixTimeMilliseconds(l)
                        : DateTimeOffset.FromUnixTimeSeconds(l);
                return dto;
            }

            if (input is int i)
            {
                var dto = DateTimeOffset.FromUnixTimeSeconds(i);
                return dto;
            }

            throw new InvalidCastException(
                $"Cannot convert '{input.GetType()}' to DateTimeOffset."
            );
        }

        var conversionProvider = new TypeConversionProvider();
        object? result = conversionProvider.Convert(typeof(object), targetType, input);
        return result;
    }
}
