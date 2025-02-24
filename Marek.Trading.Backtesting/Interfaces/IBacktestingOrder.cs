namespace Marek.Trading.Backtesting;

public interface IBacktestingOrder : IOrder
{
    void ValidateBeforePlacement(double? marketPrice = null);
    void SetPlaced(DateTime placedTime, double placedPrice);
    void SetExecuted(DateTime executionTime, double executionPrice, double feeRate);
    void SetCancelled(DateTime cancelledTime);
}
