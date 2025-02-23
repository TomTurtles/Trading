namespace Marek.Trading.Core;

public class UpdatePositionCommandBuilder
{
    private readonly Dictionary<UpdatePositionCommandType, double> _commands = [];

    public Dictionary<UpdatePositionCommandType, double> Build() => _commands;
    public UpdatePositionCommandBuilder Liquidate()
    {
        _commands.Add(UpdatePositionCommandType.Liquidate, 0d);
        return this;
    }
    public UpdatePositionCommandBuilder DecreaseSize(double relative)
    {
        _commands.Add(UpdatePositionCommandType.DecreaseSize, relative);
        return this;
    }
    public UpdatePositionCommandBuilder IncreaseSize(double relative)
    {
        _commands.Add(UpdatePositionCommandType.IncreaseSize, relative);
        return this;
    }
    public UpdatePositionCommandBuilder UpdateExitRules(double? takeProfitPrice = null, double? stopLossPrice = null)
    {
        if (takeProfitPrice == null && stopLossPrice == null) throw new ArgumentException("at least one of the parameters must be not null");

        if (takeProfitPrice != null)
        {
            _commands.Add(UpdatePositionCommandType.UpdateTakeProfitPrice, takeProfitPrice!.Value);
        }

        if (stopLossPrice != null)
        {
            _commands.Add(UpdatePositionCommandType.UpdateStopLossPrice, stopLossPrice!.Value);
        }

        return this;
    }
}
