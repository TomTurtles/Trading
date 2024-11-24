namespace Trading;

public static class Module
{
    public static IServiceCollection AddTrading(this IServiceCollection services)
    {
        return services
            .AddSingleton<IBacktestEngine, BacktestEngine>()
            .AddSingleton<IBacktestExchange, BacktestExchange>()
            .AddSingleton<IPerformanceTracker, PerformanceTracker>()
            .AddSingleton<IEventSystem, EventSystem>()
            .AddSingleton<IEventBus, EventBus>();
    }
}
