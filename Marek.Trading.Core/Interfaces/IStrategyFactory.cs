namespace Marek.Trading.Core;

public interface IStrategyFactory
{
    IStrategy CreateStrategy(string name);
}
