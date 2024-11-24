namespace Trading;
public class EventSystem() : IEventSystem
{
    private IEventBus EventBus { get; } = new EventBus();
    private JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        
    };

    public void Publish<T>(EventType eventType, T element, [CallerMemberName] string? caller = null)
    {
        var msg = JsonSerializer.Serialize(element, SerializerOptions);
        EventBus.Publish(eventType.ToString(), msg);
        Console.WriteLine($"Publish {eventType}: {msg}");
    }

    public Action<string> Subscribe<T>(EventType eventType, Action<T> onElementReceived)
    {
        if (onElementReceived is null) throw new NullReferenceException(nameof(onElementReceived));

        Action<string> actionOnMessage = msg =>
        {
            var element = JsonSerializer.Deserialize<T>(msg, SerializerOptions) ?? throw new NullReferenceException(eventType.ToString());
            onElementReceived.Invoke(element);
        };

        Console.WriteLine($"{eventType} subscribed");

        EventBus.Subscribe(eventType.ToString(), actionOnMessage);

        // use this for unsubscribing
        return actionOnMessage;
    }

    public void Unsubscribe(EventType eventType, Action<string> actionOnMessage)
    {
        if (actionOnMessage is null) throw new NullReferenceException(nameof(actionOnMessage));

        EventBus.Unsubscribe(eventType.ToString(), actionOnMessage);
    }
}
