namespace Marek.Trading.Backtesting.Tests;
public class Test1Strategy : StrategyBase
{
    public override string Name => "test1";

    public override async Task GoLongAsync(Candle candle, Order order)
    {
        order.Quantity = 1;
    }

    public override async Task GoShortAsync(Candle candle, Order order)
    {
        order.Quantity = 1;
    }

    public override Task<bool> ShouldLongAsync(Candle candle)
    {
        return Task.FromResult(new Random().Next(0, 100) > 50);
    }

    public override Task<bool> ShouldShortAsync(Candle candle)
    {
        return Task.FromResult(new Random().Next(0, 100) > 50);
    }
}
