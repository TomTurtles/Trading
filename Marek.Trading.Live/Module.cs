namespace Marek.Trading;

public static class Module
{
    public static IServiceCollection AddMarekLiveTrading(
        this IServiceCollection services, 
        IStrategy strategy, 
        IExchange exchange,
        Action<LiveTradingOptions> configure)
    {
        return services
            .AddMareator()
            .Configure<LiveTradingOptions>(configure)
            .AddSingleton<ILiveTradingEngine, LiveTradingEngine>()
            .AddSingleton<IStrategy>(provider =>
            {
                if (strategy is IStrategyInitializable initializable)
                {
                    var logger = provider.GetRequiredService<ILogger<IStrategy>>();
                    var eventDispatcher = provider.GetRequiredService<IMareatorEventDispatcher>();
                    var options = provider.GetRequiredService<IOptions<LiveTradingOptions>>();

                    initializable.Initialize(eventDispatcher, exchange, logger, options.Value);
                }

                return strategy;
            })
            .AddSingleton<IExchange>(provider =>
            {
                if (exchange is IExchangeInitializable initializable)
                {
                    var logger = provider.GetRequiredService<ILogger<IExchange>>();
                    var eventDispatcher = provider.GetRequiredService<IMareatorEventDispatcher>();
                    var options = provider.GetRequiredService<IOptions<LiveTradingOptions>>();

                    initializable.Initialize(eventDispatcher, logger, options.Value);
                }

                return exchange;
            });
    }
}


