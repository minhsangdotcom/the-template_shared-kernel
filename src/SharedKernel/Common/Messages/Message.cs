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

public class Message<T>(string? entityName = null)
    where T : class
{
    public string EntityName { get; } =
        string.IsNullOrWhiteSpace(entityName) ? typeof(T).Name : entityName;

    public string ObjectName { get; internal set; } = string.Empty;

    public string? PropertyName { get; internal set; } = string.Empty;

    public CustomMessage? CustomMessage { get; internal set; }

    public MessageType Type { get; internal set; } = 0;

    public bool? IsNegative { get; internal set; } = null;

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

        string? strMessage =
            IsNegative == true && !string.IsNullOrWhiteSpace(negativeMessage)
                ? negativeMessage
                : BuildMainRawMessage(IsNegative, message);

        results.Add(strMessage);

        if (!string.IsNullOrWhiteSpace(ObjectName))
        {
            results.Add(ObjectName.ToKebabCase());
        }

        string en = Translate(LanguageType.En);
        string vi = Translate(LanguageType.Vi);
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

        Dictionary<string, ResourceResult> translator = ResourceExtension.ReadResxFile(path) ?? [];

        ResourceResult? propertyTranslation = translator!.GetValueOrDefault(PropertyName);
        string property = propertyTranslation?.Value ?? string.Empty;
        string entity = translator!.GetValueOrDefault(EntityName)?.Value ?? string.Empty;
        string obj = translator.GetValueOrDefault(ObjectName)?.Value ?? string.Empty;

        string messagePreposition = string.Empty;
        MessageDictionary? messageDictionary = Type == 0 ? null : Messages.GetValueOrDefault(Type);
        string? message = BuildMainTranslationMessage(
            IsNegative,
            CustomMessage?.NegativeMessage ?? messageDictionary?.NegativeMessage,
            CustomMessage?.CustomMessageTranslations[languageType.ToString()]
                ?? messageDictionary?.Translation[languageType.ToString()],
            languageType
        );

        if (messageDictionary?.Preposition.HasValue == true && !string.IsNullOrWhiteSpace(obj))
        {
            messagePreposition =
                languageType == LanguageType.En
                    ? messageDictionary.Preposition!.Value.Key
                    : messageDictionary.Preposition!.Value.Value;
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

    private static string? BuildMainRawMessage(bool? isNegative, string? message)
    {
        if (isNegative == false)
        {
            return message;
        }
        string? mess = string.IsNullOrWhiteSpace(message) ? string.Empty : $"_{message}";
        return "not" + mess;
    }

    private static string? BuildMainTranslationMessage(
        bool? isNegative,
        string? negativeMessage,
        string? message,
        LanguageType languageType
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
        string localeNegativeWord = languageType == LanguageType.En ? "not" : "không";
        return string.Join(
            " ",
            new[] { localeNegativeWord, message }.Where(item => !string.IsNullOrEmpty(item))
        );
    }
}

public record CustomMessage(
    string Message,
    Dictionary<string, string> CustomMessageTranslations,
    string? NegativeMessage = null
);
