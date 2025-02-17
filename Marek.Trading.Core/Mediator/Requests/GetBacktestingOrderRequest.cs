namespace Marek.Trading.Core;

public class GetBacktestingOrderRequest(string id) : IRequest<Order?>
{
    public string Id { get; } = id;
}
