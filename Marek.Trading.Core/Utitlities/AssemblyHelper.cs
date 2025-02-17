namespace Marek.Trading.Core;

public class AssemblyHelper
{
    public static Type FindImplementationType<TInterface>(string name, params Assembly[] assemblies)
    {
        var definitionType = typeof(TInterface);

        var types = assemblies
            .SelectMany(asm => asm.GetTypes())
            .ToList();

        var typesWithName = types
            .Where(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
           
        var result = typesWithName
            .Single(t => definitionType.IsAssignableFrom(t) && (definitionType.IsAbstract || definitionType.IsInterface));

        return result;
    }
}
