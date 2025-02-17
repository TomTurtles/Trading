namespace Marek.Trading.Core;
public record ClosePositionCommand(Candle Candle, Position Position) : ICommand
{
}
