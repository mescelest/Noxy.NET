using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class EntityPlaylistExtensionTest
{
    private static EntityPlaylist CreatePlaylist(params int[] positionList)
    {
        EntityPlaylist playlist = new() { UserID = Guid.CreateVersion7(), Name = "Mix" };
        foreach (int position in positionList)
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = Guid.CreateVersion7(), Position = position });
        }

        return playlist;
    }

    [Fact]
    public void HasDuplicates_TheSameMediaFileTwice_IsTrue()
    {
        EntityPlaylist playlist = CreatePlaylist(0, 1, 2);
        playlist.EntryList[2] = new() { PlaylistID = playlist.ID, MediaFileID = playlist.EntryList[0].MediaFileID, Position = 2 };

        Assert.True(playlist.HasDuplicates());
    }

    [Fact]
    public void HasDuplicates_DistinctMediaFilesOrNoEntries_IsFalse()
    {
        Assert.False(CreatePlaylist(0, 1, 2).HasDuplicates());
        Assert.False(CreatePlaylist().HasDuplicates());
    }

    [Fact]
    public void RenumberEntryList_MakesPositionsDenseInTheOrderOfTheList()
    {
        EntityPlaylist playlist = CreatePlaylist(4, 9, 2);
        List<Guid> order = [.. playlist.EntryList.Select(x => x.ID)];

        playlist.RenumberEntryList();

        Assert.Equal([0, 1, 2], playlist.EntryList.Select(x => x.Position));
        Assert.Equal(order, playlist.EntryList.Select(x => x.ID));
    }

    [Fact]
    public void RenumberEntryList_UsesTheListOrder_NotTheOldPositions()
    {
        EntityPlaylist playlist = CreatePlaylist(5, 1);

        playlist.RenumberEntryList();

        Assert.Equal(0, playlist.EntryList[0].Position);
        Assert.Equal(1, playlist.EntryList[1].Position);
    }

    [Fact]
    public void RenumberEntryList_ReturnsOnlyTheEntriesThatChanged()
    {
        EntityPlaylist playlist = CreatePlaylist(0, 5);

        List<EntityPlaylistEntry> changed = playlist.RenumberEntryList();

        Assert.Equal([playlist.EntryList[1].ID], changed.Select(x => x.ID));
    }

    [Fact]
    public void RenumberEntryList_AlreadyDense_ChangesNothing()
    {
        Assert.Empty(CreatePlaylist(0, 1, 2).RenumberEntryList());
    }

    [Fact]
    public void RenumberEntryList_RepeatedPositions_AreSeparated()
    {
        EntityPlaylist playlist = CreatePlaylist(3, 3, 3);

        playlist.RenumberEntryList();

        Assert.Equal([0, 1, 2], playlist.EntryList.Select(x => x.Position));
    }

    [Fact]
    public void RenumberEntryList_EmptyList_ReturnsEmpty()
    {
        Assert.Empty(CreatePlaylist().RenumberEntryList());
    }

    [Fact]
    public void ToResponse_CarriesTheLastPlayedTimeAsUtc_OrNullWhenNeverPlayed()
    {
        EntityPlaylist playlist = CreatePlaylist();
        Assert.Null(playlist.ToResponse(0).TimeLastPlayed);
        Assert.Null(playlist.ToDetailResponse().TimeLastPlayed);

        // The database hands the time back without a kind.
        playlist.TimeLastPlayed = new DateTime(2026, 5, 6, 7, 8, 9, DateTimeKind.Unspecified);

        Assert.Equal(new DateTime(2026, 5, 6, 7, 8, 9, DateTimeKind.Utc), playlist.ToResponse(0).TimeLastPlayed);
        Assert.Equal(DateTimeKind.Utc, playlist.ToResponse(0).TimeLastPlayed?.Kind);
        Assert.Equal(DateTimeKind.Utc, playlist.ToDetailResponse().TimeLastPlayed?.Kind);
    }
}
