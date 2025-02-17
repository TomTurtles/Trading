namespace Marek.Trading.Core;

public interface IExchangeFactory
{
    IExchange CreateExchange(string name);
}
