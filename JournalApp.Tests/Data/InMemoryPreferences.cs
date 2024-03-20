namespace JournalApp.Tests.Data;

public sealed class InMemoryPreferences : IPreferences
{
    private readonly Dictionary<string, object> Dict = [];

    public void Clear(string sharedName = null)
    {
        if (sharedName != null)
            throw new NotImplementedException();

        Dict.Clear();
    }

    public bool ContainsKey(string key, string sharedName = null)
    {
        if (sharedName != null)
            throw new NotImplementedException();

        return Dict.ContainsKey(key);
    }

    public T Get<T>(string key, T defaultValue, string sharedName = null)
    {
        if (sharedName != null)
            throw new NotImplementedException();

        if (Dict.TryGetValue(key, out var value))
            return (T)value;

        return defaultValue;
    }

    public void Remove(string key, string sharedName = null)
    {
        if (sharedName != null)
            throw new NotImplementedException();

        Dict.Remove(key);
    }

    public void Set<T>(string key, T value, string sharedName = null)
    {
        if (sharedName != null)
            throw new NotImplementedException();

        Dict[key] = value;
    }
}
