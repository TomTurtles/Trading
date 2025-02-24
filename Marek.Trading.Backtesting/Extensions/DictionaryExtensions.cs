namespace Marek.Trading.Backtesting;

public static class DictionaryExtensions
{
    /// <summary>
    /// Fügt ein Element hinzu oder aktualisiert den vorhandenen Wert für den angegebenen Schlüssel.
    /// </summary>
    /// <typeparam name="TKey">Der Typ des Schlüssels im Dictionary.</typeparam>
    /// <typeparam name="TValue">Der Typ des Werts im Dictionary.</typeparam>
    /// <param name="dictionary">Das Dictionary, das erweitert wird.</param>
    /// <param name="key">Der Schlüssel des Elements, das hinzugefügt oder aktualisiert werden soll.</param>
    /// <param name="value">Der neue Wert, der hinzugefügt oder aktualisiert werden soll.</param>
    public static void AddOrUpdate<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TKey, TValue> addValueFunc,
        Func<TKey, TValue, TValue> updateValueFunc)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = updateValueFunc(key, dictionary[key]); 
        }
        else
        {
            dictionary.Add(key, addValueFunc(key)); 
        }
    }
}
