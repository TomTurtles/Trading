namespace Marek.Trading.Core;
public interface IStrategyInitializable
{
    void Initialize(IMareatorEventDispatcher dispatcher, IExchange exchange, ILogger<IStrategy> logger, IStrategyOptions options);
}
