namespace Marek.Trading.Core;

public class BacktestingCandleResult 
{
    public Candle Candle {  get; set; }

    [ConvertStringEnum]
    public StrategyDecisionType Decision { get; set; }
    public BacktestingAccountState? ExchangeState { get; set; }
    public IEnumerable<Position> ClosedPositions { get; set; }
    public BacktestEngineCandleStateError? Error { get; set; }


    public static BacktestingCandleResult Create(Candle candle, StrategyDecision decision, BacktestingAccountState exchangeState, IEnumerable<Position> closedPositions)
    {
        if (exchangeState.ClosedPosition != null)
        {
            closedPositions = closedPositions.Concat([exchangeState.ClosedPosition]);
        }

        return new BacktestingCandleResult()
        {
            Candle = candle,
            Decision = decision.Type,
            ExchangeState = exchangeState,
            ClosedPositions = closedPositions,
        };
    }

    public static BacktestingCandleResult WithCandle(Candle candle) => new BacktestingCandleResult() { Candle = candle };

    public static BacktestingCandleResult FromException(Candle candle, Exception ex)
    {
        return new BacktestingCandleResult()
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
