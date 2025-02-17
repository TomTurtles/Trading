namespace Marek.Trading.Core;

public class GetOrdersRequest(string symbol): IRequest<List<Order>>
{
    public string Symbol { get; } = symbol;
}
