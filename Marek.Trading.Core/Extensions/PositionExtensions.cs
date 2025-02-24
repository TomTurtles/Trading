namespace Marek.Trading.Core;
public static class PositionExtensions
{
    public static bool IsLong(this IPosition position) => position.Side == PositionSide.LONG;
    public static bool IsShort(this IPosition position) => position.Side == PositionSide.SHORT;
    public static bool IsOpen(this IPosition position) => position.Status == PositionStatus.Open;
    public static bool IsClosed(this IPosition position) => position.Status == PositionStatus.Closed;
}
