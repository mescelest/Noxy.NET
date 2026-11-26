using System.ComponentModel;
using System.Net;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class MetadataActionServiceTest
{
    private readonly FakeApiClient _client = new();
    private readonly MetadataActionService _service;

    public MetadataActionServiceTest()
    {
        _service = new(_client);
    }

    private static List<Guid> NewIDList(int count) => Enumerable.Range(0, count).Select(_ => Guid.CreateVersion7()).ToList();

    [Fact]
    public async Task Delete_WhenTheServerRefuses_IsNoLongerBusy()
    {
        _client.WriteFailure = new HttpRequestException("down");

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.Delete(MetadataKindEnum.Genre, Guid.CreateVersion7()));

        Assert.False(_service.IsBusy);
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
    public async Task SetWeight_ForAnArtistAndAGenre_SendsTheWeightToTheRightEndpoint()
    {
        Guid artist = Guid.CreateVersion7();
        Guid genre = Guid.CreateVersion7();

        await _service.SetWeight(MetadataKindEnum.Artist, artist, 700);
        await _service.SetWeight(MetadataKindEnum.Genre, genre, 20);

        Assert.Equal((artist, 700), (_client.SetArtistWeightRequestList.Single().ID, _client.SetArtistWeightRequestList.Single().Request.Weight));
        Assert.Equal((genre, 20), (_client.SetGenreWeightRequestList.Single().ID, _client.SetGenreWeightRequestList.Single().Request.Weight));
    }

    [Fact]
    public async Task SetWeight_ForAnAlbum_Throws_AndSendsNothing()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => _service.SetWeight(MetadataKindEnum.Album, Guid.CreateVersion7(), 5));

        Assert.Empty(_client.SetArtistWeightRequestList);
        Assert.Empty(_client.SetGenreWeightRequestList);
        Assert.False(_service.IsBusy);
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
    public async Task AKindOutsideTheEnum_Throws()
    {
        await Assert.ThrowsAsync<InvalidEnumArgumentException>(() => _service.Delete((MetadataKindEnum)99, Guid.CreateVersion7()));
    }

    [Fact]
    public async Task SetAlbumArt_SendsThePicture_AndIsBusyUntilTheAnswer()
    {
        Guid album = Guid.CreateVersion7();
        List<bool> seenList = [];
        _service.BusyChanged += () => seenList.Add(_service.IsBusy);

        await _service.SetAlbumArt(album, [1, 2], "image/png");

        (Guid id, byte[] picture, string contentType) = Assert.Single(_client.SetAlbumArtRequestList);
        Assert.Equal(album, id);
        Assert.Equal([1, 2], picture);
        Assert.Equal("image/png", contentType);
        Assert.Equal([true, false], seenList);
    }

    [Fact]
    public async Task SetAlbumArt_WhenTheServerRefuses_Throws_AndIsNoLongerBusy()
    {
        _client.WriteFailure = new ApiException(HttpStatusCode.BadRequest, "Not an image.");

        await Assert.ThrowsAsync<ApiException>(() => _service.SetAlbumArt(Guid.CreateVersion7(), [1], "image/png"));

        Assert.False(_service.IsBusy);
    }

    [Fact]
    public async Task Apply_SendsEachChange_AsAnAddOrARemove_AndIsBusyOnceForAll()
    {
        Guid artistA = Guid.CreateVersion7();
        Guid artistB = Guid.CreateVersion7();
        List<Guid> addList = NewIDList(2);
        List<Guid> removeList = NewIDList(1);
        List<bool> seenList = [];
        _service.BusyChanged += () => seenList.Add(_service.IsBusy);

        await _service.Apply(MetadataKindEnum.Artist, [new(artistA, true, addList), new(artistB, false, removeList)]);

        Assert.Equal((artistA, addList), (Assert.Single(_client.AddArtistMediaFileListRequestList).ID, _client.AddArtistMediaFileListRequestList[0].Request.MediaFileIDList));
        Assert.Equal((artistB, removeList), (Assert.Single(_client.RemoveArtistMediaFileListRequestList).ID, _client.RemoveArtistMediaFileListRequestList[0].Request.MediaFileIDList));
        Assert.Equal([true, false], seenList);
    }

    [Fact]
    public async Task Apply_Album_SetsAndClearsThroughTheAlbumCalls()
    {
        Guid setAlbum = Guid.CreateVersion7();
        Guid clearedAlbum = Guid.CreateVersion7();

        await _service.Apply(MetadataKindEnum.Album, [new(setAlbum, true, NewIDList(1)), new(clearedAlbum, false, NewIDList(1))]);

        Assert.Equal(setAlbum, Assert.Single(_client.AddAlbumMediaFileListRequestList).ID);
        Assert.Equal(clearedAlbum, Assert.Single(_client.RemoveAlbumMediaFileListRequestList).ID);
        Assert.Empty(_client.AddArtistMediaFileListRequestList);
        Assert.Empty(_client.AddGenreMediaFileListRequestList);
    }

    [Fact]
    public async Task Apply_Genre_UsesTheGenreCalls()
    {
        await _service.Apply(MetadataKindEnum.Genre, [new(Guid.CreateVersion7(), true, NewIDList(1)), new(Guid.CreateVersion7(), false, NewIDList(1))]);

        Assert.Single(_client.AddGenreMediaFileListRequestList);
        Assert.Single(_client.RemoveGenreMediaFileListRequestList);
    }

    [Fact]
    public async Task Apply_WhenOneRequestIsRefused_Throws_AndSendsNoMore()
    {
        _client.WriteFailure = new ApiException(HttpStatusCode.NotFound, "Gone.");

        await Assert.ThrowsAsync<ApiException>(() => _service.Apply(MetadataKindEnum.Genre, [new(Guid.CreateVersion7(), true, NewIDList(1)), new(Guid.CreateVersion7(), false, NewIDList(1))]));

        Assert.Single(_client.AddGenreMediaFileListRequestList);
        Assert.Empty(_client.RemoveGenreMediaFileListRequestList);
        Assert.False(_service.IsBusy);
    }

    [Fact]
    public async Task Apply_WithAChangeOverTheLimit_ThrowsBeforeSendingAnything()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.Apply(MetadataKindEnum.Artist, [new(Guid.CreateVersion7(), true, NewIDList(1)), new(Guid.CreateVersion7(), true, NewIDList(1001))]));

        Assert.Empty(_client.AddArtistMediaFileListRequestList);
    }

    [Fact]
    public async Task Apply_WithExactlyTheLimit_SendsOneRequest()
    {
        await _service.Apply(MetadataKindEnum.Artist, [new(Guid.CreateVersion7(), true, NewIDList(1000))]);

        Assert.Equal(1000, Assert.Single(_client.AddArtistMediaFileListRequestList).Request.MediaFileIDList.Count);
    }

    [Fact]
    public async Task Merge_Album_SendsTheTarget_AndIsBusyUntilTheAnswer()
    {
        Guid source = Guid.CreateVersion7();
        Guid target = Guid.CreateVersion7();
        List<bool> seenList = [];
        _service.BusyChanged += () => seenList.Add(_service.IsBusy);

        await _service.Merge(MetadataKindEnum.Album, source, target);

        (Guid id, var request) = Assert.Single(_client.MergeAlbumRequestList);
        Assert.Equal(source, id);
        Assert.Equal(target, request.TargetID);
        Assert.Equal([true, false], seenList);
    }

    [Fact]
    public async Task Merge_Artist_AndGenre_UseTheirOwnCalls()
    {
        await _service.Merge(MetadataKindEnum.Artist, Guid.CreateVersion7(), Guid.CreateVersion7());
        await _service.Merge(MetadataKindEnum.Genre, Guid.CreateVersion7(), Guid.CreateVersion7());

        Assert.Single(_client.MergeArtistRequestList);
        Assert.Single(_client.MergeGenreRequestList);
        Assert.Empty(_client.MergeAlbumRequestList);
    }

    [Fact]
    public async Task Merge_WhenTheServerRefuses_Throws_AndIsNoLongerBusy()
    {
        _client.WriteFailure = new ApiException(HttpStatusCode.BadRequest, "An album cannot be merged into itself.");

        await Assert.ThrowsAsync<ApiException>(() => _service.Merge(MetadataKindEnum.Album, Guid.CreateVersion7(), Guid.CreateVersion7()));

        Assert.False(_service.IsBusy);
    }

    [Fact]
    public async Task Merge_AKindOutsideTheEnum_Throws()
    {
        await Assert.ThrowsAsync<InvalidEnumArgumentException>(() => _service.Merge((MetadataKindEnum)99, Guid.CreateVersion7(), Guid.CreateVersion7()));
    }

    [Fact]
    public async Task MergeList_MergesEachSourceIntoTheTarget_InOrder_AndIsBusyOnceForAll()
    {
        Guid target = Guid.CreateVersion7();
        List<Guid> sourceList = NewIDList(3);
        List<bool> seenList = [];
        _service.BusyChanged += () => seenList.Add(_service.IsBusy);

        await _service.MergeList(MetadataKindEnum.Album, sourceList, target);

        Assert.Equal(sourceList, _client.MergeAlbumRequestList.Select(x => x.ID));
        Assert.All(_client.MergeAlbumRequestList, x => Assert.Equal(target, x.Request.TargetID));
        Assert.Equal([true, false], seenList);
    }

    [Fact]
    public async Task MergeList_WhenOneIsRefused_Throws_AndMergesNoMore()
    {
        _client.WriteFailure = new ApiException(HttpStatusCode.NotFound, "Gone.");

        await Assert.ThrowsAsync<ApiException>(() => _service.MergeList(MetadataKindEnum.Genre, NewIDList(3), Guid.CreateVersion7()));

        Assert.Single(_client.MergeGenreRequestList);
        Assert.False(_service.IsBusy);
    }

    [Fact]
    public async Task MergeList_WithTheTargetAmongTheSources_Throws_AndSendsNothing()
    {
        Guid target = Guid.CreateVersion7();

        await Assert.ThrowsAsync<ArgumentException>(() => _service.MergeList(MetadataKindEnum.Artist, [Guid.CreateVersion7(), target], target));

        Assert.Empty(_client.MergeArtistRequestList);
    }

    [Fact]
    public async Task MergeList_WithNoSources_SendsNothing()
    {
        await _service.MergeList(MetadataKindEnum.Artist, [], Guid.CreateVersion7());

        Assert.Empty(_client.MergeArtistRequestList);
    }
}
