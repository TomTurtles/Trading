namespace Trading;
public interface IEventSystem
{
    void Publish<T>(EventType eventType, T element, [CallerMemberName] string? caller = null);
    Action<string> Subscribe<T>(EventType eventType, Action<T> onElementReceived);
    void Unsubscribe(EventType eventType, Action<string> actionOnMessage);
}
