namespace Marek.Trading.Core;

public class ExchangeList
{
    string Name { get;set; }

    private ExchangeList(string name)
    {
        Name = name;
    }

    public static ExchangeList Poloniex = new(nameof(Poloniex));

    public override string ToString() => Name;

    public static implicit operator string(ExchangeList coll) => coll.Name;
    public static implicit operator ExchangeList(string s) => new(s);
}
