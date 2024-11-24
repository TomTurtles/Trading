namespace Trading.Backtesting;
public class BacktestEngineCandleState
{
    public Candle Candle {  get; set; }
    public StrategyDecision? Decision { get; set; }
    public ExchangeState? ExchangeState { get; set; }
    public BacktestEngineCandleStateError? Error { get; set; }


    internal static BacktestEngineCandleState Create(Candle candle, StrategyDecision decision, ExchangeState exchangeState)
    {
        return new BacktestEngineCandleState()
        {
            Candle = candle,
            Decision = decision,
            ExchangeState = exchangeState
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
