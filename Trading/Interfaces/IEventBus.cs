namespace Trading;

public interface IEventBus
{
    void Publish(string channel, string value, [CallerMemberName] string? caller = null);
    void Subscribe(string channel, Action<string> action);
    void Unsubscribe(string channel, Action<string> action);
}
