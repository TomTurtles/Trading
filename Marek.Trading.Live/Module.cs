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
            .Configure<LiveTradingOptions>(configure)
            .AddSingleton<IStrategy>(provider =>
            {
                if (strategy is IStrategyInitializable initializable)
                {
                    var logger = provider.GetRequiredService<ILogger<IStrategy>>();
                    var eventDispatcher = provider.GetRequiredService<IMareatorEventDispatcher>();
                    var exchange = provider.GetRequiredService<IExchange>();
                    var options = provider.GetRequiredService<IOptions<LiveTradingOptions>>();

                    //initializable.Initialize(eventDispatcher, exchange, logger, options.Value);
                }

                return strategy;
            })
            .AddSingleton<IExchange>(provider =>
            {
                if (exchange is IExchangeInitializable exchangeInitializable)
                {
                    var eventDispatcher = provider.GetRequiredService<IMareatorEventDispatcher>();
                    var logger = provider.GetRequiredService<ILogger<IExchange>>();
                    var options = provider.GetRequiredService<IOptions<LiveTradingOptions>>();
                    //exchangeInitializable.Initialize(eventDispatcher, logger, options.Value);
                }

                return exchange;
            });
    }
}


