namespace Marek.Trading.Backtesting.Tests;

public class CandleHelper
{
    public static Candle Random(DateTime? timestamp = null, CandleInterval interval = CandleInterval.Day_1)
    {
        return new Candle()
        {
            Timestamp = timestamp ?? DateTime.Now,
            Open = new Random().NextDouble() * 100,
            Close = new Random().NextDouble() * 100,
            High = new Random().NextDouble() * 100,
            Low = new Random().NextDouble() * 100,
            Volume = new Random().NextDouble() * 1000,
            Interval = interval
        };
    }
}
