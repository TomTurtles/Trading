namespace Marek.Trading.Core;

public enum EventType
{
    OnNewCandle,
    OnClosePosition,
    OnUpdatePosition,
    OnDoNothing,
    OnCancelOrders,
    OnGoLong,
    OnGoShort,
    OnStrategyException,
    OnConnectionOpened,
    OnOrderPlaced,
    OnOrderExecuted,
    OnOrderCancelled,
    OnPositionClosed,
    OnPositionOpened,
    OnPositionUpdated,
    OnMarginCall,
    OnStrategyExecuted,
    OnCashChanged
}
