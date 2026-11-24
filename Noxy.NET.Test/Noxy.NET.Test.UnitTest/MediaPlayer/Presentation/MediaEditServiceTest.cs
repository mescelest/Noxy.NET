using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Models.FormModel;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class MediaEditServiceTest
{
    private readonly FakeApiClient _api = new();
    private readonly FakeSyncService _sync = new();
    private readonly MediaEditService _service;

    public MediaEditServiceTest()
    {
        _service = new(_api, _sync);
    }

    private static ResponseMediaFile Media(string? artist = null, int? year = null)
    {
        return new()
        {
            ID = Guid.CreateVersion7(), Title = "Title", ContentType = "audio/mpeg", SizeBytes = 1, ContentHash = "h", TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow,
            Artist = artist, Year = year
        };
    }

    [Fact]
    public async Task Edit_SendsOnlyTheChangedFields_AndAppliesTheAnswer()
    {
        ResponseMediaFile media = Media("Artist", 2000);
        FormModelEditMedia model = FormModelEditMedia.From(media);
        model.Title = "New title";

        await _service.Edit(media, model);

        (Guid id, var request) = Assert.Single(_api.UpdateMediaRequestList);
        Assert.Equal(media.ID, id);
        Assert.Equal("New title", request.Title.Value);
        Assert.False(request.Artist.HasValue);
        Assert.False(request.Year.HasValue);
        Assert.Equal([nameof(FakeSyncService.ApplyMediaUpdated)], _sync.CallList);
    }

    [Fact]
    public async Task Edit_WithNothingChanged_MakesNoCall()
    {
        ResponseMediaFile media = Media("Artist", 2000);

        await _service.Edit(media, FormModelEditMedia.From(media));

        Assert.Empty(_api.UpdateMediaRequestList);
        Assert.Empty(_sync.CallList);
    }

    [Fact]
    public async Task Edit_ABlankField_IsSentAsNull_SoTheTagIsCleared()
    {
        ResponseMediaFile media = Media("Artist", 2000);
        FormModelEditMedia model = FormModelEditMedia.From(media);
        model.Artist = "";
        model.Year = null;

        await _service.Edit(media, model);

        var request = Assert.Single(_api.UpdateMediaRequestList).Request;
        Assert.True(request.Artist.HasValue);
        Assert.Null(request.Artist.Value);
        Assert.True(request.Year.HasValue);
        Assert.Null(request.Year.Value);
    }

    [Fact]
    public async Task Edit_TextIsTrimmed_AndSpacesOnlyCountsAsBlank()
    {
        ResponseMediaFile media = Media("Artist");
        FormModelEditMedia model = FormModelEditMedia.From(media);
        model.Title = "  Spaced  ";
        model.Artist = "   ";

        await _service.Edit(media, model);

        var request = Assert.Single(_api.UpdateMediaRequestList).Request;
        Assert.Equal("Spaced", request.Title.Value);
        Assert.Null(request.Artist.Value);
    }

    [Fact]
    public async Task Edit_AnUntouchedArtistText_IsNotSent()
    {
        ResponseMediaFile media = Media("A feat. B");
        FormModelEditMedia model = FormModelEditMedia.From(media);
        model.Title = "New";

        await _service.Edit(media, model);

        Assert.False(Assert.Single(_api.UpdateMediaRequestList).Request.Artist.HasValue);
    }

    [Fact]
    public async Task Edit_WhenTheServerRefuses_Throws_AndAppliesNothing()
    {
        ResponseMediaFile media = Media();
        FormModelEditMedia model = FormModelEditMedia.From(media);
        model.Title = "New";
        _api.WriteFailure = new ApiException(HttpStatusCode.NotFound, "Not found.");

        await Assert.ThrowsAsync<ApiException>(() => _service.Edit(media, model));

        Assert.Empty(_sync.CallList);
    }
}
