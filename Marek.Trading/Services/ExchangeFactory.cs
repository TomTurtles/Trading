namespace Marek.Trading;

public class ExchangeFactory(IServiceProvider serviceProvider, ILogger<ExchangeFactory> logger) : IExchangeFactory
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public ILogger<ExchangeFactory> Logger { get; } = logger;

    public IExchange CreateExchange(string name)
    {
        try
        {
            Assembly[] assemblies = [
                .. Assembly
                    .GetEntryAssembly()
                    .GetReferencedAssemblies()
                    .Select(a => Assembly.Load(a)),
                .. typeof(ExchangeFactory).Assembly
                    .GetReferencedAssemblies()
                    .Select(a => Assembly.Load(a))
            ];

            var types = assemblies
                .SelectMany(asm => asm.GetTypes())
                .ToList();

            var nameTypes = types.Where(t => t.Name.Equals($"{name}Exchange"));

            var type = nameTypes.Single(t => typeof(ExchangeBase).IsAssignableFrom(t));

            // Instanziierung der Strategie über den parameterlosen Konstruktor
            var instance = (ExchangeBase)Activator.CreateInstance(type);

            // Falls das Objekt IInitializable implementiert, wird die Initialisierung vorgenommen
            if (instance is IExchangeInitializable initializable)
            {
                // Hier wird explizit der ILogger abgerufen
                var eventDispatcher = ServiceProvider.GetRequiredService<IMareatorEventDispatcher>();
                var logger = ServiceProvider.GetRequiredService<ILogger<IExchange>>();
                var options = ServiceProvider.GetRequiredService<IOptions<BacktestingOptions>>();

                initializable.Initialize(eventDispatcher, logger, options.Value);
            }

            return instance;
        }
        catch (Exception)
        {
            throw;
        }
    }

}
