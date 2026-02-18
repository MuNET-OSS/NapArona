using NapArona.Hosting.Bot;

namespace NapArona.Hosting.Events;

/// <summary>
/// Generic wrapper class for bot events, containing the event data along with context about the bot that received it.
/// </summary>
/// <typeparam name="T">The type of the original event object. No constraint is applied to allow flexibility with various event types.</typeparam>
public class BotEvent<T>
{
    /// <summary>
    /// Gets the bot ID that received this event.
    /// </summary>
    public long SelfId { get; }

    /// <summary>
    /// Gets the original event object of type <typeparamref name="T"/>.
    /// </summary>
    public T Event { get; }

    /// <summary>
    /// Gets the bot context for replying to this event.
    /// </summary>
    public NapAronaBotContext BotContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BotEvent{T}"/> class.
    /// </summary>
    /// <param name="selfId">The bot ID that received the event.</param>
    /// <param name="event">The original event object.</param>
    /// <param name="botContext">The bot context for replying.</param>
    public BotEvent(long selfId, T @event, NapAronaBotContext botContext)
    {
        SelfId = selfId;
        Event = @event;
        BotContext = botContext;
    }
}
