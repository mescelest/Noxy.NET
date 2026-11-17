using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class ListStoreContract<T, TKey> where TKey : notnull
{
    protected static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    protected abstract IListStore<T, TKey> CreateStore();

    protected abstract TKey NewKey();

    protected abstract T MakeItem(string label, TKey key);

    protected abstract TKey KeyOf(T item);

    protected abstract string LabelOf(T item);

    private T MakeItem(string label)
    {
        return MakeItem(label, NewKey());
    }

    private async Task<List<string>> Labels(IListStore<T, TKey> store)
    {
        return [.. (await store.GetList()).Select(LabelOf)];
    }

    [Fact]
    public async Task AnEmptyStore_HasNoItemsAndNoETag()
    {
        IListStore<T, TKey> store = CreateStore();

        Assert.Empty(await store.GetList());
        Assert.Null(await store.GetETag());
    }

    [Fact]
    public async Task ReplaceList_ReplacesRatherThanMerges_AndKeepsTheOrder()
    {
        IListStore<T, TKey> store = CreateStore();
        await store.ReplaceList([MakeItem("Old")]);

        await store.ReplaceList([MakeItem("B"), MakeItem("A"), MakeItem("C")]);

        Assert.Equal(["B", "A", "C"], await Labels(store));
    }

    [Fact]
    public async Task ReplaceList_WithAnEmptyList_ClearsTheStore()
    {
        IListStore<T, TKey> store = CreateStore();
        await store.ReplaceList([MakeItem("Old")]);

        await store.ReplaceList([]);

        Assert.Empty(await store.GetList());
    }

    [Fact]
    public async Task Save_InsertsANewKeyAtTheEnd_AndReplacesAnExistingKeyInPlace()
    {
        IListStore<T, TKey> store = CreateStore();
        T first = MakeItem("First");
        await store.ReplaceList([first, MakeItem("Second")]);

        await store.Save(MakeItem("Third"));
        await store.Save(MakeItem("Renamed", KeyOf(first)));

        Assert.Equal(["Renamed", "Second", "Third"], await Labels(store));
    }

    [Fact]
    public async Task Delete_RemovesIt_AndAnUnknownKeyIsHarmless()
    {
        IListStore<T, TKey> store = CreateStore();
        T item = MakeItem("Gone");
        await store.ReplaceList([item, MakeItem("Other")]);

        await store.Delete(KeyOf(item));
        await store.Delete(NewKey());

        Assert.Equal(["Other"], await Labels(store));
    }

    [Fact]
    public async Task TheETag_RoundTrips_AndNullClearsIt()
    {
        IListStore<T, TKey> store = CreateStore();

        await store.SetETag("\"abc\"");
        Assert.Equal("\"abc\"", await store.GetETag());

        await store.SetETag(null);
        Assert.Null(await store.GetETag());
    }

    [Fact]
    public async Task TheETag_IsNotTouchedByWritingItems()
    {
        IListStore<T, TKey> store = CreateStore();
        await store.SetETag("\"abc\"");

        await store.Save(MakeItem("One"));
        await store.ReplaceList([MakeItem("Two")]);
        await store.Delete(NewKey());

        Assert.Equal("\"abc\"", await store.GetETag());
    }
}
