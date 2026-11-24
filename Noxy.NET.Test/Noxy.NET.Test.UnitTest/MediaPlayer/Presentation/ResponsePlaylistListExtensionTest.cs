using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ResponsePlaylistListExtensionTest
{
    private static readonly DateTime Now = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

    private static ResponsePlaylist Playlist(string name, DateTime? played, DateTime modified)
    {
        return new() { ID = Guid.CreateVersion7(), Name = name, AllowDuplicates = true, EntryCount = 0, TimeCreated = Now, TimeModified = modified, TimeLastPlayed = played };
    }

    [Fact]
    public void PlayedFirstNewestPlayedFirst()
    {
        List<ResponsePlaylist> list = [Playlist("old", Now.AddDays(-2), Now), Playlist("new", Now.AddDays(-1), Now)];

        Assert.Equal(["new", "old"], list.OrderMostRecentlyPlayedFirst().Select(x => x.Name));
    }

    [Fact]
    public void NeverPlayedAfterThemByModifiedTime()
    {
        List<ResponsePlaylist> list = [Playlist("never-old", null, Now.AddDays(-5)), Playlist("played", Now.AddDays(-30), Now), Playlist("never-new", null, Now.AddDays(-1))];

        Assert.Equal(["played", "never-new", "never-old"], list.OrderMostRecentlyPlayedFirst().Select(x => x.Name));
    }

    [Fact]
    public void ATiedTimeKeepsTheInputOrder()
    {
        List<ResponsePlaylist> list = [Playlist("a", Now, Now), Playlist("b", Now, Now), Playlist("c", null, Now), Playlist("d", null, Now)];

        Assert.Equal(["a", "b", "c", "d"], list.OrderMostRecentlyPlayedFirst().Select(x => x.Name));
    }

    [Fact]
    public void DoesNotChangeTheInputList()
    {
        List<ResponsePlaylist> list = [Playlist("never", null, Now), Playlist("played", Now, Now)];

        List<ResponsePlaylist> ordered = list.OrderMostRecentlyPlayedFirst();

        Assert.Equal(["never", "played"], list.Select(x => x.Name));
        Assert.NotSame(list, ordered);
    }

    [Fact]
    public void TakeRecentlyPlayed_ReturnsOnlyThePlayedOnes_NewestFirst()
    {
        List<ResponsePlaylist> list = [Playlist("never", null, Now), Playlist("old", Now.AddDays(-2), Now), Playlist("new", Now.AddDays(-1), Now)];

        Assert.Equal(["new", "old"], list.TakeRecentlyPlayed(6).Select(x => x.Name));
    }

    [Fact]
    public void TakeRecentlyPlayed_WhenNoneWasPlayed_FallsBackToTheOrderedList()
    {
        List<ResponsePlaylist> list = [Playlist("old", null, Now.AddDays(-5)), Playlist("new", null, Now.AddDays(-1))];

        Assert.Equal(["new", "old"], list.TakeRecentlyPlayed(6).Select(x => x.Name));
    }

    [Fact]
    public void TakeRecentlyPlayed_CapsAtTheCount()
    {
        List<ResponsePlaylist> list = [Playlist("a", Now, Now), Playlist("b", Now.AddDays(-1), Now), Playlist("c", Now.AddDays(-2), Now)];

        Assert.Equal(["a", "b"], list.TakeRecentlyPlayed(2).Select(x => x.Name));
    }
}
