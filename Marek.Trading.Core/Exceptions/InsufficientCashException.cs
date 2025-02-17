namespace Marek.Trading.Core;

public class InsufficientCashException(string msg) : Exception(msg)
{
}
