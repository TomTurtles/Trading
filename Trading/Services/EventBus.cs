namespace Trading;

internal class EventBus : IEventBus
{
    /// <summary>
    /// Channels, mapped to their event handlers.
    /// </summary>
    private readonly ConcurrentDictionary<string, EventHandler<EventBusMessageEventArgs>> Channels = new();

    /// <summary>
    /// Publishes a message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to publish the message to.</param>
    /// <param name="value">The message value.</param>
    /// <param name="caller">The name of the calling method (automatically captured).</param>
    public void Publish(string channel, string value, [CallerMemberName] string? caller = null)
    {
        if (!Channels.ContainsKey(channel))
        {
            Channels.TryAdd(channel, null);
        }

        Channels[channel]?.Invoke(caller, new(value));
    }

    /// <summary>
    /// Subscribes to a specific channel with a provided action.
    /// </summary>
    /// <param name="channel">The channel to subscribe to.</param>
    /// <param name="action">The action to execute when a message is published.</param>
    public void Subscribe(string channel, Action<string> action)
    {
        Channels.AddOrUpdate(
            channel,
            _ => (s, e) => action(e.Message),
            (_, existingHandler) => existingHandler + ((s, e) => action(e.Message))
        );
    }

    /// <summary>
    /// Unsubscribes from a specific channel.
    /// </summary>
    /// <param name="channel">The channel to unsubscribe from.</param>
    /// <param name="action">The action to remove from the channel.</param>
    public void Unsubscribe(string channel, Action<string> action)
    {
        if (!Channels.ContainsKey(channel)) return;

        Channels.AddOrUpdate(
            channel,
            null,
            (_, existingHandler) => existingHandler - ((s, e) => action(e.Message))
        );
    }
}