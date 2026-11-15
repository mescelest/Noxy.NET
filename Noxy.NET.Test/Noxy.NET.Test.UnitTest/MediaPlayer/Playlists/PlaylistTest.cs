using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class PlaylistTest
{
    private static Playlist CreatePlaylist(params int[] positionList)
    {
        Playlist playlist = new() { UserID = Guid.CreateVersion7(), Name = "Mix" };
        foreach (int position in positionList)
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = Guid.CreateVersion7(), Position = position });
        }

        return playlist;
    }

    [Fact]
    public void HasDuplicates_TheSameMediaFileTwice_IsTrue()
    {
        Playlist playlist = CreatePlaylist(0, 1, 2);
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
        Playlist playlist = CreatePlaylist(4, 9, 2);
        List<Guid> order = [.. playlist.EntryList.Select(x => x.ID)];

        playlist.RenumberEntryList();

        Assert.Equal([0, 1, 2], playlist.EntryList.Select(x => x.Position));
        Assert.Equal(order, playlist.EntryList.Select(x => x.ID));
    }

    [Fact]
    public void RenumberEntryList_UsesTheListOrder_NotTheOldPositions()
    {
        Playlist playlist = CreatePlaylist(5, 1);

        playlist.RenumberEntryList();

        Assert.Equal(0, playlist.EntryList[0].Position);
        Assert.Equal(1, playlist.EntryList[1].Position);
    }

    [Fact]
    public void RenumberEntryList_ReturnsOnlyTheEntriesThatChanged()
    {
        Playlist playlist = CreatePlaylist(0, 5);

        List<PlaylistEntry> changed = playlist.RenumberEntryList();

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
        Playlist playlist = CreatePlaylist(3, 3, 3);

        playlist.RenumberEntryList();

        Assert.Equal([0, 1, 2], playlist.EntryList.Select(x => x.Position));
    }

    [Fact]
    public void RenumberEntryList_EmptyList_ReturnsEmpty()
    {
        Assert.Empty(CreatePlaylist().RenumberEntryList());
    }
}
