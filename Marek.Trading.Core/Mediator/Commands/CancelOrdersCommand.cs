namespace Marek.Trading.Core;
public record CancelOrdersCommand(Candle Candle, List<Order> Orders) : ICommand
{
}
