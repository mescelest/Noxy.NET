using System.ComponentModel;
using System.Net;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class MetadataActionServiceTest
{
    private readonly FakeApiClient _client = new();
    private readonly MetadataRecentService _recent = new(new FakeDevicePreferenceStorage());
    private readonly MetadataActionService _service;

    public MetadataActionServiceTest()
    {
        _service = new(_client, _recent);
    }

    private static List<Guid> NewIDList(int count) => Enumerable.Range(0, count).Select(_ => Guid.CreateVersion7()).ToList();

    [Fact]
    public async Task Assign_IsBusyFromTheSendUntilTheAnswer_AndSaysSoOnBothChanges()
    {
        List<bool> seenList = [];
        _service.BusyChanged += () => seenList.Add(_service.IsBusy);

        await _service.Assign(MetadataKindEnum.Artist, Guid.CreateVersion7(), NewIDList(1));

        Assert.Equal([true, false], seenList);
        Assert.False(_service.IsBusy);
    }

    [Fact]
    public async Task Delete_WhenTheServerRefuses_IsNoLongerBusy()
    {
        _client.WriteFailure = new HttpRequestException("down");

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.Delete(MetadataKindEnum.Genre, Guid.CreateVersion7()));

        Assert.False(_service.IsBusy);
    }

    [Fact]
    public async Task Assign_Artist_CallsAddArtistMediaFileList_WithTheList()
    {
        Guid artist = Guid.CreateVersion7();
        List<Guid> mediaList = NewIDList(3);

        await _service.Assign(MetadataKindEnum.Artist, artist, mediaList);

        (Guid id, var request) = Assert.Single(_client.AddArtistMediaFileListRequestList);
        Assert.Equal(artist, id);
        Assert.Equal(mediaList, request.MediaFileIDList);
        Assert.Empty(_client.AddAlbumMediaFileListRequestList);
        Assert.Empty(_client.AddGenreMediaFileListRequestList);
    }

    [Fact]
    public async Task Assign_Album_CallsAddAlbumMediaFileList()
    {
        Guid album = Guid.CreateVersion7();

        await _service.Assign(MetadataKindEnum.Album, album, NewIDList(1));

        Assert.Equal(album, Assert.Single(_client.AddAlbumMediaFileListRequestList).ID);
    }

    [Fact]
    public async Task Assign_Genre_RecordsTheItemAsRecent()
    {
        Guid genre = Guid.CreateVersion7();

        await _service.Assign(MetadataKindEnum.Genre, genre, NewIDList(1));

        Assert.Equal([genre], await _recent.Get(MetadataKindEnum.Genre, new HashSet<Guid> { genre }));
    }

    [Fact]
    public async Task Assign_MoreThanTheLimit_Throws_AndSendsNothing()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.Assign(MetadataKindEnum.Artist, Guid.CreateVersion7(), NewIDList(1001)));

        Assert.Empty(_client.AddArtistMediaFileListRequestList);
    }

    [Fact]
    public async Task Assign_Exactly1000_SendsOneRequest()
    {
        await _service.Assign(MetadataKindEnum.Artist, Guid.CreateVersion7(), NewIDList(1000));

        Assert.Equal(1000, Assert.Single(_client.AddArtistMediaFileListRequestList).Request.MediaFileIDList.Count);
    }

    [Fact]
    public async Task Remove_Genre_CallsRemoveGenreMediaFileList_AndDoesNotRecord()
    {
        Guid genre = Guid.CreateVersion7();
        List<Guid> mediaList = NewIDList(2);

        await _service.Remove(MetadataKindEnum.Genre, genre, mediaList);

        (Guid id, var request) = Assert.Single(_client.RemoveGenreMediaFileListRequestList);
        Assert.Equal(genre, id);
        Assert.Equal(mediaList, request.MediaFileIDList);
        Assert.Empty(await _recent.Get(MetadataKindEnum.Genre, new HashSet<Guid> { genre }));
    }

    [Fact]
    public async Task Remove_MoreThanTheLimit_Throws_AndSendsNothing()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.Remove(MetadataKindEnum.Album, Guid.CreateVersion7(), NewIDList(1001)));

        Assert.Empty(_client.RemoveAlbumMediaFileListRequestList);
    }

    [Fact]
    public async Task Create_Album_SendsTitleAndArtistIDList_AndReturnsTheNewID()
    {
        Guid newID = Guid.CreateVersion7();
        Guid artist = Guid.CreateVersion7();
        _client.CreateAlbumHandler = request => FakeApiClient.AlbumFor(newID, request.Title, request.ArtistIDList);

        Guid result = await _service.Create(MetadataKindEnum.Album, "Homogenic", [artist]);

        Assert.Equal(newID, result);
        var request = Assert.Single(_client.CreateAlbumRequestList);
        Assert.Equal("Homogenic", request.Title);
        Assert.Equal([artist], request.ArtistIDList);
    }

    [Fact]
    public async Task Create_Artist_SendsTheName_AndReturnsTheNewID()
    {
        Guid newID = Guid.CreateVersion7();
        _client.CreateArtistHandler = request => FakeApiClient.ArtistFor(newID, request.Name);

        Guid result = await _service.Create(MetadataKindEnum.Artist, "Björk", null);

        Assert.Equal(newID, result);
        Assert.Equal("Björk", Assert.Single(_client.CreateArtistRequestList).Name);
    }

    [Fact]
    public async Task Rename_Album_SendsTheTitle_AndTheArtistsOnlyWhenGiven()
    {
        Guid album = Guid.CreateVersion7();
        Guid artist = Guid.CreateVersion7();

        await _service.Rename(MetadataKindEnum.Album, album, "New", null);
        await _service.Rename(MetadataKindEnum.Album, album, "Newer", [artist]);

        Assert.Equal(2, _client.UpdateAlbumRequestList.Count);
        Assert.False(_client.UpdateAlbumRequestList[0].Request.ArtistIDList.HasValue);
        Assert.Equal("New", _client.UpdateAlbumRequestList[0].Request.Title.Value);
        Assert.Equal([artist], _client.UpdateAlbumRequestList[1].Request.ArtistIDList.Value);
    }

    [Fact]
    public async Task Delete_Genre_CallsDeleteGenre()
    {
        Guid genre = Guid.CreateVersion7();

        await _service.Delete(MetadataKindEnum.Genre, genre);

        Assert.Equal([genre], _client.DeleteGenreList);
    }

    [Fact]
    public async Task Assign_WhenTheServerRefuses_ThrowsAndRecordsNothing()
    {
        Guid artist = Guid.CreateVersion7();
        _client.WriteFailure = new ApiException(HttpStatusCode.NotFound, "Gone.");

        await Assert.ThrowsAsync<ApiException>(() => _service.Assign(MetadataKindEnum.Artist, artist, NewIDList(1)));

        Assert.Empty(await _recent.Get(MetadataKindEnum.Artist, new HashSet<Guid> { artist }));
    }

    [Fact]
    public async Task AKindOutsideTheEnum_Throws()
    {
        await Assert.ThrowsAsync<InvalidEnumArgumentException>(() => _service.Delete((MetadataKindEnum)99, Guid.CreateVersion7()));
    }
}
