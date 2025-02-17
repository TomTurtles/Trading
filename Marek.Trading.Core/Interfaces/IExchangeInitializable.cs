namespace Marek.Trading.Core;
public interface IExchangeInitializable
{
    void Initialize(IMareatorEventDispatcher eventDispatcher, ILogger<IExchange> logger, IExchangeOptions options);
}
