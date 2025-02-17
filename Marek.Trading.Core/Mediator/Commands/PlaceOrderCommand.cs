namespace Marek.Trading.Core;
public record PlaceOrderCommand(Candle Candle, Order Order) : ICommand
{
}
