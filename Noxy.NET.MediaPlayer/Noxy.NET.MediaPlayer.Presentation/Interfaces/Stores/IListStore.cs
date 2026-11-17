namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface IListStore<T, in TKey> where TKey : notnull
{
    Task<List<T>> GetList();

    // A Replace is one atomic write that keeps the order of the list; a saved item that exists stays where it is and a new one goes last.
    Task ReplaceList(List<T> list);

    Task Save(T item);

    // A key that is not held is harmless.
    Task Delete(TKey key);

    // Only ever one that arrived with a full 200 response the device downloaded. Null clears it.
    Task<string?> GetETag();

    Task SetETag(string? eTag);
}
