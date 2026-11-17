using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class SyncedList<T, TKey>(IListStore<T, TKey> store, Func<string?, Task<ApiResponse<List<T>>>> fetch, Func<T, TKey> keyOf, Func<T, DateTime> timeModifiedOf) where TKey : notnull
{
    // Raised after the local list changed.
    public event Action? Changed;

    public async Task<List<T>> GetList()
    {
        return await store.GetList();
    }

    public async Task<T?> Find(TKey key)
    {
        return (await store.GetList()).FirstOrDefault(x => EqualityComparer<TKey>.Default.Equals(keyOf(x), key));
    }

    // Sends the stored ETag unless force is set or the local list is empty, so cleared app data is refilled instead of staying empty on a 304. A failed network call never changes the list or its ETag.
    public async Task Sync(bool force = false)
    {
        string? eTag = force || (await store.GetList()).Count == 0 ? null : await store.GetETag();

        Func<Task<ApiResponse<List<T>>>> call = () => fetch(eTag);
        ApiResponse<List<T>>? response = await call.RunOrNullWhenOffline();
        if (response?.Value is null) return;

        await store.ReplaceList(response.Value);
        await store.SetETag(response.ETag);
        Changed?.Invoke();
    }

    // Never writes an ETag. An older TimeModified never overwrites the local copy, and an equal one does not either unless equalOverwrites is set.
    public async Task ApplyUpdated(T item, bool equalOverwrites = false)
    {
        T? local = await Find(keyOf(item));
        if (local is not null && (equalOverwrites ? timeModifiedOf(item) < timeModifiedOf(local) : timeModifiedOf(item) <= timeModifiedOf(local))) return;

        await store.Save(item);
        Changed?.Invoke();
    }

    public async Task ApplyDeleted(TKey key)
    {
        await store.Delete(key);
        Changed?.Invoke();
    }
}
