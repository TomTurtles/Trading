namespace Trading.Backtesting;
public class BacktestEnginePerformanceResult
{
    public IndicatorsPerformanceResult Indicators { get; private set; }
    public BacktestPerformanceResult Backtest { get; private set; }
    public EquityPerformanceResult Equity { get; private set; }
    public AssetPerformanceResult Asset { get; private set; }
    public CashPerformanceResult Cash { get; private set; }
    public IEnumerable<BacktestEngineCandleStateError> Errors { get; private set; }

    internal static BacktestEnginePerformanceResult FromStates(string name, BacktestOptions options, IEnumerable<BacktestEngineCandleState> states, Stopwatch sw)
    {
        return new()
        {
            Indicators = new IndicatorsPerformanceResult(states),
            Backtest = new BacktestPerformanceResult(name, options, states, sw),
            Asset = new AssetPerformanceResult(states.Where(s => s.ExchangeState != null).Select(s => s.Candle.Close)),
            Cash = new CashPerformanceResult(states.Where(s => s.ExchangeState != null).Select(s => s.ExchangeState!.Cash)),
            Equity = new EquityPerformanceResult(states.Where(s => s.ExchangeState != null).Select(s => s.ExchangeState!.Equity)),
            Errors = states.Where(e => e.Error != null).Select(s => s.Error!)
        };
    }

    public class IndicatorsPerformanceResult(IEnumerable<BacktestEngineCandleState> states)
    {
        public int Trades => states.Sum(state => state.ClosedPositions?.Count() ?? 0);
        public double MaxDrawdown
        {
            get
            {
                var maxDrawdown = 0d;
                var equityCurve = states.ToDictionary(state => state.Candle.Timestamp, state => state.ExchangeState!.Equity);

                var peak = equityCurve.First().Value;

                foreach (var (date, equity) in equityCurve)
                {
                    if (equity > peak) peak = equity;
                    var drawdown = (peak - equity) / peak;
                    if (drawdown > maxDrawdown) maxDrawdown = drawdown;
                }

                return Math.Round(-maxDrawdown * 100, 2);
            }
        }

        public double WinRate 
        { 
            get
            {
                var closedPositions = states.SelectMany(state => state.ClosedPositions);
                if (!closedPositions.Any()) Math.Round(0d, 2);

                var winRate = (double)closedPositions.Count(cp => cp.PNL is not null && cp.PNL > 0d) / closedPositions.Count();
                return Math.Round(winRate * 100, 2);
            }
        }

        public double AvgWin => Math.Round(states.SelectMany(state => state.ClosedPositions).Where(cp => cp.PNL > 0).Average(cp => cp.PNL) ?? 0, 2);
        public double AvgLoss => Math.Round(states.SelectMany(state => state.ClosedPositions).Where(cp => cp.PNL <= 0).Average(cp => cp.PNL) ?? 0, 2);

        public double ExcessReturnRatio
        {
            get
            {
                var priceList = states.Select(s => s.Candle.Close);
                var equityList = states.Where(s => s.ExchangeState != null).Select(s => s.ExchangeState!.Equity);

                var priceRatio = priceList.Last() / priceList.First();
                var equityRatio = equityList.Last() / equityList.First();

                return Math.Round(equityRatio / priceRatio, 2);
            }
        }
    }

    public class AssetPerformanceResult(IEnumerable<double> priceList)
    {
        public double Start => priceList.First();
        public double End => priceList.Last();
        public double Min => priceList.Min();
        public double Max => priceList.Max();
        public string Performance => ((End - Start) / Start).ToString("0.00%");
    }

    public class BacktestPerformanceResult(string name, BacktestOptions options, IEnumerable<BacktestEngineCandleState> states, Stopwatch sw)
    {
        public string Strategy => name;
        public int Candles => states.Count();
        public string Symbol => options.Symbol;

        [ConvertStringEnum]
        public CandleInterval Interval => options.Interval;
        public DateTime Start => options.StartAt ?? states.First().Candle.Timestamp;
        public DateTime End => options.EndAt ?? states.Last().Candle.Timestamp;
        public TimeSpan Duration => sw.Elapsed;
    }

    public class CashPerformanceResult(IEnumerable<double> cashList)
    {
        public double Start => cashList.First();
        public double End => cashList.Last();
        public double Min => cashList.Min();
        public double Max => cashList.Max();
        public string Performance => ((End - Start) / Start).ToString("0.00%");
    }

    public class EquityPerformanceResult(IEnumerable<double> equityList)
    {
        public double Start => equityList.First();
        public double End => equityList.Last();
        public double Min => equityList.Min();
        public double Max => equityList.Max();
        public string Performance => ((End - Start) / Start).ToString("0.00%");
    }
}