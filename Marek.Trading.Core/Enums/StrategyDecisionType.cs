namespace Marek.Trading.Core;

public enum StrategyDecisionType
{
    Wait,
    GoLong,
    GoShort,
    CancelOrders,
    UpdatePosition,
    ClosePosition,
    Error,
    Start,
}
