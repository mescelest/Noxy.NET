using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Requests.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class RequestPlaylistEntryTest
{
    private static List<ValidationResult> Validate(object request)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(request, new(request), results, true);
        return results;
    }

    private static List<Guid> Ids(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Guid.CreateVersion7())];
    }

    [Fact]
    public void Add_AValidRequest_IsValid()
    {
        Assert.Empty(Validate(new RequestAddPlaylistEntry { SongIDList = Ids(2), Position = 3 }));
        Assert.Empty(Validate(new RequestAddPlaylistEntry { SongIDList = Ids(1) }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Add_AnEmptyOrTooLongList_IsInvalid(int count)
    {
        Assert.NotEmpty(Validate(new RequestAddPlaylistEntry { SongIDList = Ids(count) }));
    }

    [Fact]
    public void Add_ANegativePosition_IsInvalid()
    {
        Assert.NotEmpty(Validate(new RequestAddPlaylistEntry { SongIDList = Ids(1), Position = -1 }));
    }

    [Fact]
    public void Remove_AValidRequest_IsValid()
    {
        Assert.Empty(Validate(new RequestRemovePlaylistEntry { EntryIDList = Ids(2) }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Remove_AnEmptyOrTooLongList_IsInvalid(int count)
    {
        Assert.NotEmpty(Validate(new RequestRemovePlaylistEntry { EntryIDList = Ids(count) }));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(-1, false)]
    public void Move_ThePositionCannotBeNegative(int position, bool valid)
    {
        Assert.Equal(valid, Validate(new RequestMovePlaylistEntry { Position = position }).Count == 0);
    }
}
