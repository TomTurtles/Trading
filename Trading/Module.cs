namespace Trading;

public static class Module
{
    public static IServiceCollection AddTrading(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEventSystem, EventSystem>()
            .AddSingleton<IEventBus, EventBus>();
    }
}
