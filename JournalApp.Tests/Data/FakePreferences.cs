namespace JournalApp.Tests.Data;

public sealed class FakePreferences : IPreferences
{
    private readonly Dictionary<string, string> Dict = [];

    public void Clear(string sharedName = null) => Dict.Clear();
    public bool ContainsKey(string key, string sharedName = null) => Dict.ContainsKey(key);
    public T Get<T>(string key, T defaultValue, string sharedName = null) => defaultValue;
    public void Remove(string key, string sharedName = null) { }
    public void Set<T>(string key, T value, string sharedName = null) { }
}
