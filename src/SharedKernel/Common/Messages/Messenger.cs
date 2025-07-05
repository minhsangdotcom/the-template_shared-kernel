using System.Linq.Expressions;
using SharedKernel.Extensions.Expressions;

namespace SharedKernel.Common.Messages;

public static class Messenger
{
    public static Message<T> Create<T>(string? objectName = null)
        where T : class => new(objectName);

    public static Message<T> Property<T>(this Message<T> message, Expression<Func<T, object>> prop)
        where T : class
    {
        message.PropertyName = prop.ToStringProperty();
        return message;
    }

    public static Message<T> Property<T>(this Message<T> message, string propertyName)
        where T : class
    {
        message.PropertyName = propertyName;
        return message;
    }

    public static Message<T> CustomProperty<T>(
        this Message<T> message,
        string propertyName,
        string? additions = null
    )
        where T : class
    {
        message.PropertyName = propertyName;
        message.Additions = additions;
        return message;
    }

    public static Message<T> Negative<T>(this Message<T> message, bool isNegative = true)
        where T : class
    {
        message.IsNegative = isNegative;
        return message;
    }

    public static Message<T> Object<T>(this Message<T> message, string name)
        where T : class
    {
        message.ObjectName = name;
        return message;
    }

    /// <summary>
    /// Set the custom message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="customMessage"></param>
    /// <returns></returns>
    public static Message<T> Message<T>(this Message<T> message, CustomMessage customMessage)
        where T : class
    {
        message.CustomMessage = customMessage;
        return message;
    }

    public static Message<T> Message<T>(this Message<T> message, MessageType type)
        where T : class
    {
        message.Type = type;
        return message;
    }

    /// <summary>
    /// if the automatic translator did not work well, try your own one for english
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="englishMessage"></param>
    /// <returns></returns>
    public static Message<T> EnglishTranslation<T>(this Message<T> message, string englishMessage)
        where T : class
    {
        message.EnglishTranslatedMessage = englishMessage;
        return message;
    }

    /// <summary>
    /// if the automatic translator did not work well, try your own one for Vietnamese
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="englishMessage"></param>
    /// <returns></returns>
    public static Message<T> VietnameseTranslation<T>(
        this Message<T> message,
        string vietnameseMessage
    )
        where T : class
    {
        message.VietnameseTranslatedMessage = vietnameseMessage;
        return message;
    }

    /// <summary>
    /// build the completed message.
    /// make sure you put it at the very end.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <returns></returns>
    public static MessageResult Build<T>(this Message<T> message)
        where T : class => message.BuildMessage();
}
