namespace SharedKernel.Common.Messages;

public class MessageDictionary(
    string message,
    Dictionary<string, string> translation,
    MessageType type,
    string? negativeMessage = null,
    KeyValuePair<string, string>? preposition = null
)
{
    /// <summary>
    /// Origin message
    /// </summary>
    public string Message { get; set; } = message ?? throw new ArgumentException($"{message} ");

    public Dictionary<string, string> Translation { get; set; } =
        translation ?? throw new ArgumentException($"{message} ");

    /// <summary>
    /// a meaningful negative message instead of using (not + message)
    /// </summary>
    public string? NegativeMessage { get; set; } = negativeMessage;

    public MessageType Type { get; set; } = type;

    public KeyValuePair<string, string>? Preposition { get; set; } = preposition;
}
