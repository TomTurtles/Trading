using System.Linq;

namespace Trading;

public class StrategyDecision
{
    public StrategyDecisionType Type { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, object> Parameters { get; set; }


    internal static StrategyDecision Wait(string reason, params (string key, object value)[] values) => new()
    {
        Type = StrategyDecisionType.Wait,
        Reason = reason,
        Parameters = values.ToDictionary(v => v.key, v => v.value),
    };

    internal static StrategyDecision ClosePosition(Position position) => new()
    {
        Type = StrategyDecisionType.ClosePosition,
        Parameters = new Dictionary<string, object>() 
        {
            { nameof(position), position },
        },
    };

    internal static StrategyDecision UpdatePosition(Position position, Action<Position> configureNewPosition) => new()
    {
        Type = StrategyDecisionType.UpdatePosition,
        Parameters = new Dictionary<string, object>() 
        { 
            { nameof(position), position }, 
            { nameof(configureNewPosition), configureNewPosition } 
        },
    };

    internal static StrategyDecision CancelOrders(IEnumerable<Order> orders) => new()
    {
        Type = StrategyDecisionType.CancelOrders,
        Parameters = new Dictionary<string, object>()
        {
            { nameof(orders), orders },
        },
    };

    internal static StrategyDecision GoLong(Order order) => new()
    {
        Type = StrategyDecisionType.GoLong,
        Parameters = new Dictionary<string, object>()
        {
            { nameof(order), order },
        },
    };

    internal static StrategyDecision GoShort(Order order) => new()
    {
        Type = StrategyDecisionType.GoShort,
        Parameters = new Dictionary<string, object>()
        {
            { nameof(order), order },
        },
    };

    internal static StrategyDecision Error(Exception exception) => new()
    {
        Type = StrategyDecisionType.Error,
        Parameters = new Dictionary<string, object>()
        {
            { nameof(exception), exception },
        },
    };

    public T Get<T>(string key)
    {
        var obj = Parameters[key];
        return obj is null
            ? throw new NullReferenceException($"no parameter with key '{key}'")
            : (T)obj ?? throw new InvalidCastException($"casting to '{nameof(T)} did not work as expected'");
    }
}
