namespace Marek.Trading.Tests;

public class TestSettings
{
    public static ServiceProvider ServiceProvider => new ServiceCollection()
        .AddLogging()
        .AddMarekBacktesting(options =>
        {
            options.Strategy = "test1";
            options.DataFeed = "test1";
            options.Symbol = "BTC_USDT";
            options.InitialCash = 10000;
            options.MarginCallLevel = 1000;
        },
        provider =>
        {
            var strategyProvider = new ServiceCollection()
                .AddSingleton<IStrategy, Test1Strategy>()
                .AddSingleton<IStrategy, Test2Strategy>()
                .AddSingleton<IStrategy, Test3Strategy>()
                .BuildServiceProvider();

            var strategies = strategyProvider.GetServices<IStrategy>();
            var options = provider.GetRequiredService<IOptions<BacktestingOptions>>();

            var strategy = strategies
                .Single(s => s.Name.Equals(options.Value.Strategy, StringComparison.OrdinalIgnoreCase));

            if (strategy is IStrategyInitializable initializable)
            {
                var logger = provider.GetRequiredService<ILogger<IStrategy>>();
                var eventDispatcher = provider.GetRequiredService<IMareatorEventDispatcher>();
                var exchange = provider.GetRequiredService<IBacktestingExchange>();

                initializable.Initialize(eventDispatcher, exchange, logger, options.Value);
            }

            return strategy;
        },
        provider =>
        {
            var datafeedProvider = new ServiceCollection()
                .AddSingleton<IDataFeedExchange, Test1DataFeed>()
                .AddSingleton<IDataFeedExchange, Test2DataFeed>()
                .BuildServiceProvider();

            var exchanges = datafeedProvider.GetServices<IDataFeedExchange>();
            var options = provider.GetRequiredService<IOptions<BacktestingOptions>>();

            var exchange = exchanges
                .Single(s => s.Name.Equals(options.Value.DataFeed, StringComparison.OrdinalIgnoreCase));

            if (exchange is IExchangeInitializable exchangeInitializable)
            {
                var eventDispatcher = ServiceProvider.GetRequiredService<IMareatorEventDispatcher>();
                var logger = provider.GetRequiredService<ILogger<IExchange>>();
                exchangeInitializable.Initialize(eventDispatcher, logger, options.Value);
            } 

            return exchange;
        })
        .BuildServiceProvider();
}
