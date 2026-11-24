using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ResponsePlaylistListExtensionTest
{
    private static readonly DateTime Now = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

    private static ResponsePlaylist Playlist(string name, DateTime? played, DateTime created, int position = 0)
    {
        return new() { ID = Guid.CreateVersion7(), Name = name, AllowDuplicates = true, Position = position, EntryCount = 0, TimeCreated = created, TimeModified = created, TimeLastPlayed = played };
    }

    [Fact]
    public void Played_NewestPlayedFirst()
    {
        List<ResponsePlaylist> list = [Playlist("old", Now.AddDays(-2), Now.AddDays(-30)), Playlist("new", Now.AddDays(-1), Now.AddDays(-30))];

        Assert.Equal(["new", "old"], list.OrderMostRecentFirst().Select(x => x.Name));
    }

    [Fact]
    public void ANewPlaylist_ComesBeforeOnesPlayedBeforeItWasCreated()
    {
        List<ResponsePlaylist> list = [Playlist("played", Now.AddDays(-1), Now.AddDays(-30)), Playlist("just-made", null, Now)];

        Assert.Equal(["just-made", "played"], list.OrderMostRecentFirst().Select(x => x.Name));
    }

    [Fact]
    public void ANewPlaylist_FallsBehindOneThatIsPlayedAfterItWasCreated()
    {
        List<ResponsePlaylist> list = [Playlist("made-earlier", null, Now.AddDays(-2)), Playlist("played-later", Now, Now.AddDays(-30))];

        Assert.Equal(["played-later", "made-earlier"], list.OrderMostRecentFirst().Select(x => x.Name));
    }

    [Fact]
    public void NeverPlayed_NewestCreatedFirst()
    {
        List<ResponsePlaylist> list = [Playlist("never-old", null, Now.AddDays(-5)), Playlist("never-new", null, Now.AddDays(-1))];

        Assert.Equal(["never-new", "never-old"], list.OrderMostRecentFirst().Select(x => x.Name));
    }

    [Fact]
    public void ATiedTimeKeepsTheInputOrder()
    {
        List<ResponsePlaylist> list = [Playlist("a", Now, Now), Playlist("b", Now, Now), Playlist("c", null, Now), Playlist("d", null, Now)];

        Assert.Equal(["a", "b", "c", "d"], list.OrderMostRecentFirst().Select(x => x.Name));
    }

    [Fact]
    public void DoesNotChangeTheInputList()
    {
        List<ResponsePlaylist> list = [Playlist("never", null, Now.AddDays(-1)), Playlist("played", Now, Now.AddDays(-30))];

        List<ResponsePlaylist> ordered = list.OrderMostRecentFirst();

        Assert.Equal(["never", "played"], list.Select(x => x.Name));
        Assert.NotSame(list, ordered);
    }

    [Fact]
    public void OrderByPosition_SortsAscending_IgnoringTimes_AndKeepsTheInputOrderOnATie()
    {
        List<ResponsePlaylist> list = [Playlist("third", Now, Now, 2), Playlist("tie-a", null, Now.AddDays(-1), 1), Playlist("tie-b", null, Now.AddDays(-9), 1), Playlist("first", null, Now.AddDays(-30), 0)];

        List<ResponsePlaylist> ordered = list.OrderByPosition();

        Assert.Equal(["first", "tie-a", "tie-b", "third"], ordered.Select(x => x.Name));
        Assert.Equal(["third", "tie-a", "tie-b", "first"], list.Select(x => x.Name));
    }

    [Fact]
    public void TakeRecentlyPlayed_ReturnsOnlyThePlayedOnes_NewestFirst()
    {
        List<ResponsePlaylist> list = [Playlist("never", null, Now), Playlist("old", Now.AddDays(-2), Now.AddDays(-30)), Playlist("new", Now.AddDays(-1), Now.AddDays(-30))];

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

    [Fact]
    public void FilterByName_KeepsTheNamesThatContainTheText_IgnoringCaseAndKeepingTheOrder()
    {
        List<ResponsePlaylist> list = [Playlist("Road Trip", null, Now), Playlist("Gym", null, Now), Playlist("trip hop", null, Now)];

        Assert.Equal(["Road Trip", "trip hop"], list.FilterByName("TRIP").Select(x => x.Name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FilterByName_WithNoText_KeepsEverything(string? search)
    {
        List<ResponsePlaylist> list = [Playlist("a", null, Now), Playlist("b", null, Now)];

        Assert.Equal(["a", "b"], list.FilterByName(search).Select(x => x.Name));
    }

    [Fact]
    public void ArtHue_IsTheSameForThePlaylistEachTime_AndWithinTheColourWheel()
    {
        ResponsePlaylist playlist = Playlist("a", null, Now);

        Assert.Equal(playlist.ArtHue, playlist.ArtHue);
        Assert.InRange(playlist.ArtHue, 0, 359);
    }
}
