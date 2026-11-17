using System.Text.Json;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeListStore<T, TKey>(Func<T, TKey> keyOf) : IListStore<T, TKey> where TKey : notnull
{
    private readonly List<T> _list = [];
    private string? _eTag;

    // Everything goes in and out as a copy through JSON, as it does with a real store, so a test can never change what is stored by changing what it got back.
    private static TValue Copy<TValue>(TValue value)
    {
        return JsonSerializer.Deserialize<TValue>(JsonSerializer.Serialize(value, JsonSerializerOptions.Web), JsonSerializerOptions.Web) ?? throw new InvalidOperationException($"A {typeof(TValue).Name} did not survive a copy.");
    }

    public Task<List<T>> GetList()
    {
        return Task.FromResult(Copy(_list));
    }

    public Task ReplaceList(List<T> list)
    {
        List<T> copy = Copy(list);
        _list.Clear();
        _list.AddRange(copy);

        return Task.CompletedTask;
    }

    public Task Save(T item)
    {
        T copy = Copy(item);
        int index = _list.FindIndex(x => EqualityComparer<TKey>.Default.Equals(keyOf(x), keyOf(copy)));
        if (index >= 0) _list[index] = copy;
        else _list.Add(copy);

        return Task.CompletedTask;
    }

    public Task Delete(TKey key)
    {
        _list.RemoveAll(x => EqualityComparer<TKey>.Default.Equals(keyOf(x), key));

        return Task.CompletedTask;
    }

    public Task<string?> GetETag()
    {
        return Task.FromResult(_eTag);
    }

    public Task SetETag(string? eTag)
    {
        _eTag = eTag;

        return Task.CompletedTask;
    }
}
