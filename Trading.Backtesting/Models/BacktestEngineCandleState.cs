namespace Trading.Backtesting;
public class BacktestEngineCandleState
{
    public DateTime Timestamp {  get; set; }

    [ConvertStringEnum]
    public StrategyDecisionType Decision { get; set; }
    public ExchangeState? ExchangeState { get; set; }
    public BacktestEngineCandleStateError? Error { get; set; }


    internal static BacktestEngineCandleState Create(Candle candle, StrategyDecision decision, ExchangeState exchangeState)
    {
        return new BacktestEngineCandleState()
        {
            Timestamp = candle.Timestamp,
            Decision = decision.Type,
            ExchangeState = exchangeState
        };
    }

    internal static BacktestEngineCandleState FromException(Candle candle, Exception ex)
    {
        return new BacktestEngineCandleState()
        {
            Timestamp = candle.Timestamp,
            Error = new BacktestEngineCandleStateError()
            {
                Message = ex.Message,
                Details = ex.StackTrace,
            }
        };
    }
}
