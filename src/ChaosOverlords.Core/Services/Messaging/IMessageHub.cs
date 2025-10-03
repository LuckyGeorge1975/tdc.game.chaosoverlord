namespace ChaosOverlords.Core.Services.Messaging;

/// <summary>
///     Lightweight publish/subscribe hub for decoupled communication between services and view models.
/// </summary>
public interface IMessageHub
{
    /// <summary>
    ///     Subscribes to messages of the specified type.
    /// </summary>
    /// <typeparam name="TMessage">The message payload type.</typeparam>
    /// <param name="handler">Handler invoked whenever a message of the specified type is published.</param>
    /// <returns>An <see cref="IDisposable" /> that removes the subscription when disposed.</returns>
    IDisposable Subscribe<TMessage>(Action<TMessage> handler);

    /// <summary>
    ///     Publishes the specified message to all registered subscribers.
    /// </summary>
    /// <typeparam name="TMessage">The message payload type.</typeparam>
    /// <param name="message">The message instance to publish.</param>
    void Publish<TMessage>(TMessage message);
}