using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Storage.Stores;

public class SqliteListStore<T, TKey>(SqliteMetadataStore store, string table, string eTagKey, Func<T, TKey> keyOf) : IListStore<T, TKey> where TKey : notnull
{
    public async Task<List<T>> GetList()
    {
        return await store.ReadList<T>(table);
    }

    public async Task ReplaceList(List<T> list)
    {
        await store.ReplaceTable(table, list, x => KeyText(keyOf(x)));
    }

    public async Task Save(T item)
    {
        await store.Upsert(table, KeyText(keyOf(item)), item);
    }

    public async Task Delete(TKey key)
    {
        await store.Delete(table, KeyText(key));
    }

    public async Task<string?> GetETag()
    {
        return await store.GetSetting(eTagKey);
    }

    public async Task SetETag(string? eTag)
    {
        await store.SetSetting(eTagKey, eTag);
    }

    private static string KeyText(TKey key)
    {
        return key.ToString() ?? throw new InvalidOperationException($"A {typeof(TKey).Name} key has no text form.");
    }
}
