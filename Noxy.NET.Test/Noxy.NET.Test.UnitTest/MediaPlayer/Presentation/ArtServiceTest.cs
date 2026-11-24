using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ArtServiceTest
{
    private static readonly DateTime FirstVersion = new(2026, 11, 24, 10, 0, 0, DateTimeKind.Utc);
    private static readonly byte[] Bytes = [0xFF, 0xD8, 1, 2, 3];
    private const string Prefix = "data:image/jpeg;base64,";

    private readonly FakeApiClient _api = new();
    private readonly FakeArtCacheStore _cache = new();
    private readonly FakeMetadataStore _store = new();
    private readonly FakeMetadataSyncService _sync = new();
    private readonly ArtService _service;

    public ArtServiceTest()
    {
        _api.AlbumArtHandler = _ => Bytes;
        _service = new(_api, _cache, _store, _sync);
    }

    private static string Uri(byte[] bytes)
    {
        return Prefix + Convert.ToBase64String(bytes);
    }

    private async Task<ResponseAlbum> SeedAlbum(DateTime? timeArtModified)
    {
        ResponseAlbum album = new() { ID = Guid.CreateVersion7(), Title = "A", ArtistIDList = [], MediaFileCount = 1, TimeModified = FirstVersion, TimeArtModified = timeArtModified };
        await _store.Album.Save(album);
        return album;
    }

    private async Task SetArtTime(ResponseAlbum album, DateTime? timeArtModified)
    {
        await _store.Album.Save(new() { ID = album.ID, Title = album.Title, ArtistIDList = [], MediaFileCount = 1, TimeModified = FirstVersion, TimeArtModified = timeArtModified });
    }

    [Fact]
    public async Task GetAlbumArtUri_AnAlbumWithNoArtTime_IsNull_AndMakesNoRequest()
    {
        ResponseAlbum album = await SeedAlbum(null);

        Assert.Null(await _service.GetAlbumArtUri(album.ID));

        Assert.Empty(_api.AlbumArtRequestList);
        Assert.Equal(0, _cache.SaveCount);
    }

    [Fact]
    public async Task GetAlbumArtUri_AnUnknownAlbum_IsNull_AndMakesNoRequest()
    {
        Assert.Null(await _service.GetAlbumArtUri(Guid.CreateVersion7()));

        Assert.Empty(_api.AlbumArtRequestList);
    }

    [Fact]
    public async Task GetAlbumArtUri_TheFirstCall_FetchesSavesAndReturnsTheDataUri()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);

        string? uri = await _service.GetAlbumArtUri(album.ID);

        Assert.Equal(Uri(Bytes), uri);
        Assert.Equal([album.ID], _api.AlbumArtRequestList);
        Assert.Equal((FirstVersion, Bytes), _cache.CopyList[album.ID]);
    }

    [Fact]
    public async Task GetAlbumArtUri_ASecondCallOfTheSameVersion_MakesNoRequest()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);
        await _service.GetAlbumArtUri(album.ID);

        string? uri = await _service.GetAlbumArtUri(album.ID);

        Assert.Equal(Uri(Bytes), uri);
        Assert.Single(_api.AlbumArtRequestList);
    }

    [Fact]
    public async Task GetAlbumArtUri_ACopyOnTheDevice_IsReadWithoutARequest_ByAFreshService()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);
        await _service.GetAlbumArtUri(album.ID);
        ArtService restarted = new(_api, _cache, _store, _sync);

        string? uri = await restarted.GetAlbumArtUri(album.ID);

        Assert.Equal(Uri(Bytes), uri);
        Assert.Single(_api.AlbumArtRequestList);
    }

    [Fact]
    public async Task GetAlbumArtUri_ANewerVersion_FetchesAgain_AndReplacesTheCopy()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);
        await _service.GetAlbumArtUri(album.ID);
        byte[] newer = [0xFF, 0xD8, 9, 9];
        _api.AlbumArtHandler = _ => newer;
        DateTime second = FirstVersion.AddHours(1);
        await SetArtTime(album, second);

        string? uri = await _service.GetAlbumArtUri(album.ID);

        Assert.Equal(Uri(newer), uri);
        Assert.Equal(2, _api.AlbumArtRequestList.Count);
        Assert.Equal((second, newer), _cache.CopyList[album.ID]);
    }

    [Fact]
    public async Task GetAlbumArtUri_A404_IsNull_KeepsTheOldCopy_AndIsNotAskedAgainForThatVersion()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);
        await _service.GetAlbumArtUri(album.ID);
        DateTime second = FirstVersion.AddHours(1);
        await SetArtTime(album, second);
        _api.AlbumArtHandler = _ => null;

        string? first = await _service.GetAlbumArtUri(album.ID);
        string? again = await _service.GetAlbumArtUri(album.ID);

        Assert.Null(first);
        Assert.Null(again);
        Assert.Equal(2, _api.AlbumArtRequestList.Count);
        Assert.Equal((FirstVersion, Bytes), _cache.CopyList[album.ID]);
        Assert.Empty(_cache.DeletedList);
    }

    [Fact]
    public async Task GetAlbumArtUri_AThrownFailure_IsNull_KeepsTheOldCopy_AndIsTriedAgainNextTime()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);
        await _service.GetAlbumArtUri(album.ID);
        await SetArtTime(album, FirstVersion.AddHours(1));
        _api.AlbumArtHandler = _ => throw new HttpRequestException("The network is down.");

        Assert.Null(await _service.GetAlbumArtUri(album.ID));
        _api.AlbumArtHandler = _ => Bytes;

        Assert.Equal(Uri(Bytes), await _service.GetAlbumArtUri(album.ID));
        Assert.Equal((FirstVersion.AddHours(1), Bytes), _cache.CopyList[album.ID]);
    }

    [Fact]
    public async Task GetAlbumArtUri_ManyCallsForOneAlbumAtOnce_MakeOneRequest()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);
        TaskCompletionSource gate = new();
        _api.AlbumArtGate = gate.Task;

        List<Task<string?>> callList = [.. Enumerable.Range(0, 20).Select(_ => _service.GetAlbumArtUri(album.ID))];
        gate.SetResult();
        string?[] resultList = await Task.WhenAll(callList);

        Assert.Single(_api.AlbumArtRequestList);
        Assert.All(resultList, x => Assert.Equal(Uri(Bytes), x));
    }

    [Fact]
    public async Task Changed_IsRaised_WhenAnAlbumsArtTimeChangesInTheMetadataSync()
    {
        ResponseAlbum album = await SeedAlbum(null);
        await _service.GetAlbumArtUri(album.ID);
        TaskCompletionSource raised = new();
        _service.Changed += () => raised.TrySetResult();

        await SetArtTime(album, FirstVersion);
        _sync.RaiseChanged();

        await raised.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(Uri(Bytes), await _service.GetAlbumArtUri(album.ID));
    }

    [Fact]
    public async Task Changed_IsNotRaised_WhenNoArtTimeChanged()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);
        await _service.GetAlbumArtUri(album.ID);
        int raisedCount = 0;
        _service.Changed += () => Interlocked.Increment(ref raisedCount);

        _sync.RaiseChanged();
        await _service.GetAlbumArtUri(album.ID);

        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public async Task Changed_AnAlbumThatIsGone_HasItsCopyDeleted()
    {
        ResponseAlbum album = await SeedAlbum(FirstVersion);
        await _service.GetAlbumArtUri(album.ID);
        TaskCompletionSource raised = new();
        _service.Changed += () => raised.TrySetResult();

        await _store.Album.Delete(album.ID);
        _sync.RaiseChanged();

        await raised.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal([album.ID], _cache.DeletedList);
    }
}
