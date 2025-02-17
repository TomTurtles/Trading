namespace Marek.Trading.Core;

public interface IBacktestingStrategy
{
    string Name { get;}
    Task RunAsync(Candle candle, CancellationToken cancellationToken = default);
}
