namespace Marek.Trading.Core;

public class OnAccountReportEventArgs(Candle candle, double equity, double cash) : EventArgs
{
    public Candle Candle { get; } = candle;
    public double Equity { get; } = equity;
    public double Cash { get; } = cash;
}
