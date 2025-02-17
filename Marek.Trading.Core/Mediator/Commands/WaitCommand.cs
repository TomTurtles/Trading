namespace Marek.Trading.Core;
public record WaitCommand(Candle Candle, string Reason) : ICommand
{
}
