
namespace Marek.Trading.Backtesting.Tests;
internal class Test2Strategy : IStrategy
{
    public string Name => "test2";

    public Task RunAsync(Candle candle, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
