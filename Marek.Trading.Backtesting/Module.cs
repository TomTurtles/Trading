namespace Marek.Trading;

public static class Module
{
    public static IServiceCollection AddMarekBacktesting<TStrategy, TDataFeedExchange>(
        this IServiceCollection services,
        Action<BacktestingOptions> configure,
        Func<IServiceProvider, TStrategy> strategyFactory,
        Func<IServiceProvider, TDataFeedExchange> dataFeedExchangeFactory,
        params Type[] types)
        where TStrategy : IStrategy
        where TDataFeedExchange : IDataFeedExchange
    {
        types = [
            ..types,
            typeof(BacktestingDataFeed),
            typeof(BacktestingExchange),
        ];

        return services
            .AddMareator(types)
            .Configure<BacktestingOptions>(configure)
            .AddSingleton<IBacktestingEngine, BacktestingEngine>()
            .AddSingleton<IBacktestingDataFeed, BacktestingDataFeed>()
            .AddSingleton<IBacktestingExchange, BacktestingExchange>()
            .AddSingleton<IBacktestingOrderManagement, BacktestingOrderManagement>()
            .AddSingleton<IBacktestingPositionManagement, BacktestingPositionManagement>()
            .AddSingleton<IBacktestingCashManagement, BacktestingCashManagement>()
            .AddScoped<IBacktestingPerformanceTracker, BacktestingPerformanceTracker>()
            .AddSingleton<IStrategy>(provider => strategyFactory(provider))
            .AddSingleton<IDataFeedExchange>(provider => dataFeedExchangeFactory(provider));
    }

    public static IServiceCollection AddMarekBacktesting<TStrategy, TDataFeedExchange>(
        this IServiceCollection services,
        Action<BacktestingOptions> configure,
        TStrategy strategy,
        TDataFeedExchange dataFeedExchange,
        params Type[] types)
        where TStrategy : IStrategy
        where TDataFeedExchange : IDataFeedExchange
    {
        types = [
            ..types,
            typeof(BacktestingDataFeed),
            typeof(BacktestingExchange),
        ];

        return services
            .AddMareator(types)
            .Configure<BacktestingOptions>(configure)
            .AddSingleton<IBacktestingEngine, BacktestingEngine>()
            .AddSingleton<IBacktestingDataFeed, BacktestingDataFeed>()
            .AddSingleton<IBacktestingExchange, BacktestingExchange>()
            .AddSingleton<IBacktestingOrderManagement, BacktestingOrderManagement>()
            .AddSingleton<IBacktestingPositionManagement, BacktestingPositionManagement>()
            .AddSingleton<IBacktestingCashManagement, BacktestingCashManagement>()
            .AddScoped<IBacktestingPerformanceTracker, BacktestingPerformanceTracker>()
            .AddSingleton<IStrategy>(provider =>
            {
                if (strategy is IStrategyInitializable initializable)
                {
                    var logger = provider.GetRequiredService<ILogger<IStrategy>>();
                    var eventDispatcher = provider.GetRequiredService<IMareatorEventDispatcher>();
                    var exchange = provider.GetRequiredService<IBacktestingExchange>();
                    var options = provider.GetRequiredService<IOptions<BacktestingOptions>>();

                    initializable.Initialize(eventDispatcher, exchange, logger, options.Value);
                }

                return strategy;
            })
            .AddSingleton<IDataFeedExchange>(provider =>
            {
                if (dataFeedExchange is IExchangeInitializable exchangeInitializable)
                {
                    var eventDispatcher = provider.GetRequiredService<IMareatorEventDispatcher>();
                    var logger = provider.GetRequiredService<ILogger<IExchange>>();
                    var options = provider.GetRequiredService<IOptions<BacktestingOptions>>();
                    exchangeInitializable.Initialize(eventDispatcher, logger, options.Value);
                }

                return dataFeedExchange;
            });
    }
}
