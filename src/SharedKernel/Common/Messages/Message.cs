using System.Xml.Linq;
using CaseConverter;
using SharedKernel.Extensions;

namespace SharedKernel.Common.Messages;

public static class Message
{
    public const string SUCCESS = "Success";

    public const string LOGIN_SUCCESS = $"Login {nameof(SUCCESS)}";

    public const string UNAUTHORIZED = "Unauthorized";

    public const string FORBIDDEN = "Forbidden";

    public const string TOKEN_EXPIRED = "Token expired";
}

/// <summary>
/// The structure entity + property + negative(if any) + message + object(if any)
/// </summary>
/// <typeparam name="T">must be class</typeparam>
/// <param name="entityName">the display name that represents for entity</param>
public class Message<T>(string? entityName = null)
    where T : class
{
    /// <summary>
    /// entity name, it must be translated at Message.en.resx and Message.vi.resx
    /// </summary>
    public string EntityName { get; } =
        string.IsNullOrWhiteSpace(entityName) ? typeof(T).Name : entityName;

    /// <summary>
    /// property name, it must be translated at Message.en.resx and Message.vi.resx
    /// </summary>
    public string? PropertyName { get; internal set; } = string.Empty;

    /// <summary>
    /// the object name for some cases, it must be translated at Message.en.resx and Message.vi.resx
    /// ex End date is greater than start date(object)
    /// </summary>
    public string ObjectName { get; internal set; } = string.Empty;

    /// <summary>
    /// it's like comment section in resx file but manually
    /// </summary>
    public string? Additions { get; internal set; }

    /// <summary>
    /// the message, it's not in message store.
    /// </summary>
    public CustomMessage? CustomMessage { get; internal set; }

    public MessageType Type { get; internal set; } = 0;

    /// <summary>
    /// not or
    /// </summary>
    public bool IsNegative { get; internal set; }

    public string EnglishTranslatedMessage { get; internal set; } = string.Empty;
    public string VietnameseTranslatedMessage { get; internal set; } = string.Empty;

    private readonly Dictionary<MessageType, MessageDictionary> Messages =
        ErrorMessage.ErrorMessages;

    public MessageResult BuildMessage()
    {
        string subjectProperty = EntityName.ToKebabCase();
        if (!string.IsNullOrWhiteSpace(PropertyName))
        {
            subjectProperty += $"_{PropertyName.ToKebabCase()}";
        }

        List<string?> results = [subjectProperty];

        string? message =
            CustomMessage?.Message?.ToKebabCase() ?? Messages.GetValueOrDefault(Type)?.Message;
        string? negativeMessage =
            CustomMessage?.NegativeMessage ?? Messages.GetValueOrDefault(Type)?.NegativeMessage;

        string? toBeMessageCombination =
            IsNegative && !string.IsNullOrWhiteSpace(negativeMessage)
                ? negativeMessage
                : BuildMainRawMessage(IsNegative, message);

        results.Add(toBeMessageCombination);

        if (!string.IsNullOrWhiteSpace(ObjectName))
        {
            results.Add(ObjectName.ToKebabCase());
        }

        string en = !string.IsNullOrWhiteSpace(EnglishTranslatedMessage)
            ? EnglishTranslatedMessage
            : Translate(LanguageType.En);

        string vi = !string.IsNullOrWhiteSpace(VietnameseTranslatedMessage)
            ? VietnameseTranslatedMessage
            : Translate(LanguageType.Vi);

        return new MessageResult
        {
            Message = string.Join("_", results).ToLower(),
            En = en,
            Vi = vi,
        };
    }

    private string Translate(LanguageType languageType)
    {
        string rootPath = Path.Join(Directory.GetCurrentDirectory(), "Resources");
        string path = languageType switch
        {
            LanguageType.Vi => Path.Join(rootPath, "Translations", "Message.vi.resx"),
            LanguageType.En => Path.Join(rootPath, "Translations", "Message.en.resx"),
            _ => string.Empty,
        };

        Dictionary<string, ResourceResult> translator = ReadResxFile(path) ?? [];

        ResourceResult? propertyTranslation = translator!.GetValueOrDefault(PropertyName);
        string property = propertyTranslation?.Value ?? string.Empty;
        string entity = translator!.GetValueOrDefault(EntityName)?.Value ?? string.Empty;
        string obj = translator.GetValueOrDefault(ObjectName)?.Value ?? string.Empty;

        string messagePreposition = string.Empty;
        MessageDictionary? messageDictionary = Type == 0 ? null : Messages.GetValueOrDefault(Type);

        string? negativeToBeTranslation = null;
        if (languageType == LanguageType.Vi)
        {
            string[]? comment = !string.IsNullOrWhiteSpace(Additions)
                ? Additions?.Trim()?.Split(",")
                : propertyTranslation?.Comment?.Trim()?.Split(",");

            string? translation = comment?.FirstOrDefault(x =>
            {
                string[] parts = x.Split("=");
                return parts.Length > 0 && parts[0] == "ViToBeTranslation";
            });
            string[]? values = translation?.Split("=");
            negativeToBeTranslation = values?.Length > 0 ? values[1] : null;
        }

        string? message = BuildMainTranslationMessage(
            IsNegative,
            CustomMessage?.NegativeMessage ?? messageDictionary?.NegativeMessage,
            CustomMessage?.CustomMessageTranslations[languageType.ToString()]
                ?? messageDictionary?.Translation[languageType.ToString()],
            languageType,
            negativeToBeTranslation
        );

        if (
            (
                CustomMessage?.Preposition.HasValue == true
                || messageDictionary?.Preposition.HasValue == true
            ) && !string.IsNullOrWhiteSpace(obj)
        )
        {
            messagePreposition =
                languageType == LanguageType.En
                    ? CustomMessage?.Preposition?.Key ?? messageDictionary?.Preposition?.Key ?? ""
                    : CustomMessage?.Preposition?.Value
                        ?? messageDictionary?.Preposition?.Value
                        ?? "";
        }

        string verb = string.Empty;
        if (languageType == LanguageType.En)
        {
            string[]? comment = propertyTranslation?.Comment?.Trim()?.Split(",");

            bool isPlural =
                comment != null
                && comment.Any(x =>
                {
                    string[] parts = x.Split("=");
                    return parts[0] == "IsPlural" && parts[1] == "true";
                });

            verb = isPlural ? "are" : "is";
        }

        string preposition = !string.IsNullOrWhiteSpace(property)
            ? languageType == LanguageType.En
                ? "of"
                : "của"
            : string.Empty;

        List<string> words =
        [
            property,
            preposition,
            entity,
            verb,
            message,
            messagePreposition,
            obj,
        ];
        IReadOnlyCollection<string> notEmptyWord = words.FindAll(word =>
            !string.IsNullOrWhiteSpace(word)
        );
        return string.Join(' ', notEmptyWord);
    }

    private static string? BuildMainRawMessage(bool isNegative, string? message)
    {
        if (isNegative == false)
        {
            return message;
        }
        string? mess = string.IsNullOrWhiteSpace(message) ? string.Empty : $"_{message}";
        return "not" + mess;
    }

    // get the translation of to be verb + message
    // en : not found
    // không tìm thấy
    private static string? BuildMainTranslationMessage(
        bool isNegative,
        string? negativeMessage,
        string? message,
        LanguageType languageType,
        string? viTranslation = null
    )
    {
        if (languageType == LanguageType.En && !string.IsNullOrWhiteSpace(negativeMessage))
        {
            return negativeMessage;
        }

        if (isNegative == false)
        {
            return message;
        }
        string localeNegativeWord =
            languageType == LanguageType.En ? "not" : viTranslation ?? "không";
        return string.Join(
            " ",
            new[] { localeNegativeWord, message }.Where(item => !string.IsNullOrEmpty(item))
        );
    }

    public static Dictionary<string, ResourceResult> ReadResxFile(string filePath)
    {
        try
        {
            XDocument resxDoc = XDocument.Load(filePath);
            Dictionary<string, ResourceResult> dataElements = resxDoc
                .Root!.Elements("data")
                .Select(elem => new KeyValuePair<string, ResourceResult>(
                    elem.Attribute("name")?.Value!,
                    new ResourceResult(
                        elem.Attribute("name")?.Value!,
                        elem.Element("value")?.Value!,
                        elem.Element("comment")?.Value
                    )
                ))
                .ToDictionary();

            return dataElements;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading the resx file: {ex.Message}");
        }
    }
}

public record ResourceResult(string Key, string Value, string? Comment);

public record CustomMessage(
    string Message,
    Dictionary<string, string> CustomMessageTranslations,
    string? NegativeMessage = null,
    KeyValuePair<string, string>? Preposition = null
);
