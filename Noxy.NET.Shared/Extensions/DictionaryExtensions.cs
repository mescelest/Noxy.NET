namespace Noxy.NET.Extensions;

public static class DictionaryExtensions
{
    public static bool DictionaryEqual<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)
    {
        return source.Count == target.Count && source.All(kvp => target.TryGetValue(kvp.Key, out TValue? value) && EqualityComparer<TValue>.Default.Equals(kvp.Value, value));
    }
}
