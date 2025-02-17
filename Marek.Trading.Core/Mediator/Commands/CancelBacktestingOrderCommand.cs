namespace Marek.Trading.Core;
public class CancelBacktestingOrderCommand(DateTime timestamp, string id) : ICommand
{
    public DateTime Timestamp { get; } = timestamp;
    public string Id { get; } = id;
}
