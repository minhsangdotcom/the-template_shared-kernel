using CaseConverter;

namespace SharedKernel.Common.Messages;

public static class ErrorMessage
{
    public static readonly Dictionary<MessageType, MessageDictionary> ErrorMessages =
        new()
        {
            {
                MessageType.MaximumLength,
                new(
                    "too-long",
                    new Dictionary<string, string>()
                    {
                        { LanguageType.En.ToString(), "too long" },
                        { LanguageType.Vi.ToString(), "quá dài" },
                    },
                    MessageType.MaximumLength
                )
            },
            {
                MessageType.MinimumLength,
                new(
                    "too-short",
                    new Dictionary<string, string>()
                    {
                        { LanguageType.En.ToString(), "too short" },
                        { LanguageType.Vi.ToString(), "quá ngắn" },
                    },
                    MessageType.MinimumLength
                )
            },
            {
                MessageType.Valid,
                new(
                    MessageType.Valid.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "valid" },
                        { LanguageType.Vi.ToString(), "hợp lệ" },
                    },
                    MessageType.Valid,
                    "invalid",
                    new("for", "cho")
                )
            },
            {
                MessageType.Found,
                new(
                    MessageType.Found.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "found" },
                        { LanguageType.Vi.ToString(), "tìm thấy" },
                    },
                    MessageType.Found
                )
            },
            {
                MessageType.Existence,
                new(
                    MessageType.Existence.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        {
                            LanguageType.En.ToString(),
                            MessageType.Existence.ToString().ToKebabCase()
                        },
                        { LanguageType.Vi.ToString(), "tồn tại" },
                    },
                    MessageType.Existence
                )
            },
            {
                MessageType.Correct,
                new(
                    MessageType.Correct.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        {
                            LanguageType.En.ToString(),
                            MessageType.Correct.ToString().ToKebabCase()
                        },
                        { LanguageType.Vi.ToString(), "đúng" },
                    },
                    MessageType.Correct,
                    "incorrect"
                )
            },
            {
                MessageType.Active,
                new(
                    MessageType.Active.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "active" },
                        { LanguageType.Vi.ToString(), "hoạt động" },
                    },
                    MessageType.Active,
                    "inactive"
                )
            },
            {
                MessageType.AmongTheAllowedOptions,
                new(
                    MessageType.AmongTheAllowedOptions.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "among the allowed options" },
                        { LanguageType.Vi.ToString(), "nằm trong lựa chọn cho phép" },
                    },
                    MessageType.AmongTheAllowedOptions
                )
            },
            {
                MessageType.GreaterThan,
                new(
                    MessageType.GreaterThan.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "greater than" },
                        { LanguageType.Vi.ToString(), "lớn hơn" },
                    },
                    MessageType.GreaterThan
                )
            },
            {
                MessageType.GreaterThanEqual,
                new(
                    MessageType.GreaterThanEqual.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "greater than or equal" },
                        { LanguageType.Vi.ToString(), "lớn hơn hoặc bằng" },
                    },
                    MessageType.GreaterThanEqual
                )
            },
            {
                MessageType.LessThan,
                new(
                    MessageType.LessThan.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "less than" },
                        { LanguageType.Vi.ToString(), "nhỏ hơn" },
                    },
                    MessageType.LessThan
                )
            },
            {
                MessageType.LessThanEqual,
                new(
                    MessageType.LessThanEqual.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "less than or equal" },
                        { LanguageType.Vi.ToString(), "nhỏ hơn hoặc bằng" },
                    },
                    MessageType.LessThanEqual
                )
            },
            {
                MessageType.Null,
                new(
                    MessageType.Null.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "null" },
                        { LanguageType.Vi.ToString(), "rỗng" },
                    },
                    MessageType.Null
                )
            },
            {
                MessageType.Empty,
                new(
                    MessageType.Empty.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "empty" },
                        { LanguageType.Vi.ToString(), "trống" },
                    },
                    MessageType.Empty
                )
            },
            {
                MessageType.Unique,
                new(
                    MessageType.Unique.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "unique" },
                        { LanguageType.Vi.ToString(), "là duy nhất" },
                    },
                    MessageType.Unique
                )
            },
            {
                MessageType.Strong,
                new(
                    MessageType.Strong.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "strong enough" },
                        { LanguageType.Vi.ToString(), "đủ mạnh" },
                    },
                    MessageType.Strong,
                    "weak"
                )
            },
            {
                MessageType.Expired,
                new(
                    MessageType.Expired.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "Expired" },
                        { LanguageType.Vi.ToString(), "Quá hạn" },
                    },
                    MessageType.Expired
                )
            },
            {
                MessageType.Redundant,
                new(
                    MessageType.Redundant.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "Redundant" },
                        { LanguageType.Vi.ToString(), "Dư thừa" },
                    },
                    MessageType.Redundant
                )
            },
            {
                MessageType.Missing,
                new(
                    MessageType.Missing.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "missing" },
                        { LanguageType.Vi.ToString(), "thiếu" },
                    },
                    MessageType.Missing
                )
            },
            {
                MessageType.Identical,
                new(
                    MessageType.Identical.ToString().ToKebabCase(),
                    new Dictionary<string, string>
                    {
                        { LanguageType.En.ToString(), "Identical" },
                        { LanguageType.Vi.ToString(), "Giống" },
                    },
                    MessageType.Identical,
                    preposition: new("to", "với")
                )
            },
        };
}
