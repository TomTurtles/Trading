namespace Marek.Trading.Core;

public interface IStrategy 
{
    string Name { get; }
    Task RunAsync(Candle candle, CancellationToken cancellationToken = default);
}
