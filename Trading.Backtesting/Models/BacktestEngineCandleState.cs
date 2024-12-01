namespace Trading.Backtesting;
public class BacktestEngineCandleState
{
    public Candle Candle {  get; set; }

    [ConvertStringEnum]
    public StrategyDecisionType Decision { get; set; }
    public ExchangeState? ExchangeState { get; set; }
    public IEnumerable<Position> ClosedPositions { get; set; }
    public BacktestEngineCandleStateError? Error { get; set; }


    internal static BacktestEngineCandleState Create(Candle candle, StrategyDecision decision, ExchangeState exchangeState, IEnumerable<Position> closedPositions)
    {
        if (exchangeState.ClosedPosition != null)
        {
            closedPositions = closedPositions.Concat([exchangeState.ClosedPosition]);
        }

        return new BacktestEngineCandleState()
        {
            Candle = candle,
            Decision = decision.Type,
            ExchangeState = exchangeState,
            ClosedPositions = closedPositions,
        };
    }

    internal static BacktestEngineCandleState FromException(Candle candle, Exception ex)
    {
        return new BacktestEngineCandleState()
        {
            Candle = candle,
            Error = new BacktestEngineCandleStateError()
            {
                Message = ex.Message,
                Details = ex.StackTrace,
            }
        };
    }
}
