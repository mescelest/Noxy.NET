using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Remote;

public class RequestPlayerCommandTest
{
    private static List<ValidationResult> Validate(RequestPlayerCommand command)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(command, new(command), results, true);
        return results;
    }

    [Theory]
    [InlineData(PlayerCommandEnum.Play)]
    [InlineData(PlayerCommandEnum.Pause)]
    [InlineData(PlayerCommandEnum.Next)]
    [InlineData(PlayerCommandEnum.Previous)]
    public void TheCommandsWithNoArguments_AreValidAlone(PlayerCommandEnum command)
    {
        Assert.Empty(Validate(new() { Command = command }));
    }

    [Fact]
    public void Seek_WithoutAPosition_IsInvalid()
    {
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.Seek }));
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.Seek, PositionSeconds = -1 }));
        Assert.Empty(Validate(new() { Command = PlayerCommandEnum.Seek, PositionSeconds = 12.5 }));
    }

    [Fact]
    public void JumpTo_WithoutAnItemID_IsInvalid()
    {
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.JumpTo }));
        Assert.Empty(Validate(new() { Command = PlayerCommandEnum.JumpTo, ItemID = Guid.CreateVersion7() }));
    }

    [Fact]
    public void JumpTo_KeepSkippedDefaultsToFalse()
    {
        Assert.False(new RequestPlayerCommand { Command = PlayerCommandEnum.JumpTo, ItemID = Guid.CreateVersion7() }.KeepSkipped ?? false);
    }

    [Fact]
    public void SetVolume_Above100OrBelow0_OrMissing_IsInvalid()
    {
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.SetVolume }));
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.SetVolume, Volume = 101 }));
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.SetVolume, Volume = -1 }));
        Assert.Empty(Validate(new() { Command = PlayerCommandEnum.SetVolume, Volume = 0 }));
        Assert.Empty(Validate(new() { Command = PlayerCommandEnum.SetVolume, Volume = 100 }));
    }

    [Fact]
    public void SetShuffleAndSetRepeat_RequireTheirMode()
    {
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.SetShuffle }));
        Assert.Empty(Validate(new() { Command = PlayerCommandEnum.SetShuffle, Shuffle = ShuffleModeEnum.Random }));
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.SetRepeat }));
        Assert.Empty(Validate(new() { Command = PlayerCommandEnum.SetRepeat, Repeat = RepeatModeEnum.Once }));
    }

    [Fact]
    public void PlayLibrary_RequiresAMediaFileIDAndASort()
    {
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.PlayLibrary }));
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.PlayLibrary, MediaFileID = Guid.CreateVersion7() }));
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.PlayLibrary, Sort = LibrarySortEnum.Title }));
        Assert.Empty(Validate(new() { Command = PlayerCommandEnum.PlayLibrary, MediaFileID = Guid.CreateVersion7(), Sort = LibrarySortEnum.Title }));
    }

    [Fact]
    public void PlayPlaylist_RequiresAPlaylistID()
    {
        Assert.NotEmpty(Validate(new() { Command = PlayerCommandEnum.PlayPlaylist }));
        Assert.Empty(Validate(new() { Command = PlayerCommandEnum.PlayPlaylist, PlaylistID = Guid.CreateVersion7() }));
    }

    [Fact]
    public void ANameThatIsNotACommand_FailsToBind()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<RequestPlayerCommand>("""{"command":"Explode"}""", JsonSerializerOptions.Web));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<RequestPlayerCommand>("""{"command":1}""", JsonSerializerOptions.Web));
        Assert.Equal(PlayerCommandEnum.Seek, JsonSerializer.Deserialize<RequestPlayerCommand>("""{"command":"Seek","positionSeconds":3}""", JsonSerializerOptions.Web)?.Command);
    }

    [Fact]
    public void AnUndefinedCommand_ThrowsAnInvalidEnumException_WhenValidated()
    {
        Assert.Throws<InvalidEnumArgumentException>(() => Validate(new() { Command = (PlayerCommandEnum)999 }));
    }
}
