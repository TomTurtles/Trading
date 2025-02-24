namespace Marek.Trading.Backtesting;

public class InsufficientCashException(string msg) : Exception(msg)
{
}
