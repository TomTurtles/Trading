public static class ObjectExtensions
{
    /// <summary>
    /// Rekursive Methode zum Extrahieren von Properties und deren Werten als Key-Value-Pair.
    /// </summary>
    /// <param name="obj">Das zu analysierende Objekt</param>
    /// <param name="prefix">Optionaler Prefix für geschachtelte Objekte</param>
    /// <returns>Dictionary mit allen Property-Namen und Werten</returns>
    public static Dictionary<string, object> ToKeyValuePairs(this object obj, string prefix = "")
    {
        if (obj == null)
            return new Dictionary<string, object>();

        var result = new Dictionary<string, object>();

        // Alle öffentlichen Eigenschaften des Objekts holen
        foreach (PropertyInfo prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            object value = prop.GetValue(obj);
            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

            if (value == null)
            {
                result[key] = "null";
            }
            else if (IsSimpleType(value.GetType()))
            {
                result[key] = value;
            }
            else if (value is IEnumerable enumerable && !(value is string))
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    foreach (var kvp in item.ToKeyValuePairs($"{key}[{index}]"))
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                    index++;
                }
            }
            else
            {
                foreach (var kvp in value.ToKeyValuePairs(key))
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Prüft, ob ein Typ ein primitiver Typ oder String ist.
    /// </summary>
    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsValueType || type == typeof(string) || type == typeof(DateTime);
    }
}

