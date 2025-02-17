namespace Marek.Trading;

public static class Module
{
    public static IServiceCollection AddMarekBacktesting<TStrategy, TDataFeedExchange>(
        this IServiceCollection services, 
        Action<BacktestingOptions> configure,
        Func<IServiceProvider, TStrategy> strategyFactory,
        Func<IServiceProvider, TDataFeedExchange> dataFeedExchangeFactory,
        params Type[] types)
        where TStrategy : IBacktestingStrategy
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
            .AddSingleton<IBacktestingPerformanceTracker, BacktestingPerformanceTracker>()
            .AddSingleton<IBacktestingStrategy>(p => strategyFactory(p))
            .AddSingleton<IDataFeedExchange>(p => dataFeedExchangeFactory(p));
    }

    public static IServiceCollection AddMarekLiveTrading(this IServiceCollection services, IStrategy strategy, Action<MarekLiveTradingOptions> configure)
    {
        return services
            .Configure<MarekLiveTradingOptions>(configure)
            .AddTransient<IStrategy>((provider) => strategy);
    }
}


