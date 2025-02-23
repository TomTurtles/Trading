namespace Marek.Trading.Core;

public enum UpdatePositionCommandType
{
    Liquidate = -2,
    DecreaseSize = -1,
    UpdateTakeProfitPrice = 0,
    UpdateStopLossPrice = 1,
    IncreaseSize = 2,
}
