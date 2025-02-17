namespace Marek.Trading.Tests;
internal class Test3Strategy : IStrategy
{
    public string Name => "test3";
    public Task RunAsync(Candle candle, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
