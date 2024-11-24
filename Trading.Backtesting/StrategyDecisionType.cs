namespace Trading.Backtesting;

public enum StrategyDecisionType
{
    Wait,
    GoLong,
    GoShort,
    CancelOrders,
    UpdatePosition,
    ClosePosition,
}
