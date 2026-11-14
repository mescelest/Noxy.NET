using Noxy.NET.MediaPlayer.Application.Helpers;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class PlaylistEntryOrderTest
{
    private static PlaylistEntry Entry(int position)
    {
        return new()
        {
            PlaylistID = Guid.CreateVersion7(),
            MediaFileID = Guid.CreateVersion7(),
            Position = position
        };
    }

    [Fact]
    public void Renumber_MakesPositionsDenseInTheOrderOfTheList()
    {
        List<PlaylistEntry> list = [Entry(4), Entry(9), Entry(2)];
        List<Guid> order = [.. list.Select(x => x.ID)];

        PlaylistEntryOrder.Renumber(list);

        Assert.Equal([0, 1, 2], list.Select(x => x.Position));
        Assert.Equal(order, list.Select(x => x.ID));
    }

    [Fact]
    public void Renumber_UsesTheListOrder_NotTheOldPositions()
    {
        PlaylistEntry first = Entry(5);
        PlaylistEntry second = Entry(1);

        PlaylistEntryOrder.Renumber([first, second]);

        Assert.Equal(0, first.Position);
        Assert.Equal(1, second.Position);
    }

    [Fact]
    public void Renumber_ReturnsOnlyTheEntriesThatChanged()
    {
        PlaylistEntry kept = Entry(0);
        PlaylistEntry moved = Entry(5);

        List<PlaylistEntry> changed = PlaylistEntryOrder.Renumber([kept, moved]);

        Assert.Equal([moved.ID], changed.Select(x => x.ID));
    }

    [Fact]
    public void Renumber_AlreadyDense_ChangesNothing()
    {
        List<PlaylistEntry> changed = PlaylistEntryOrder.Renumber([Entry(0), Entry(1), Entry(2)]);

        Assert.Empty(changed);
    }

    [Fact]
    public void Renumber_RepeatedPositions_AreSeparated()
    {
        List<PlaylistEntry> list = [Entry(3), Entry(3), Entry(3)];

        PlaylistEntryOrder.Renumber(list);

        Assert.Equal([0, 1, 2], list.Select(x => x.Position));
    }

    [Fact]
    public void Renumber_EmptyList_ReturnsEmpty()
    {
        Assert.Empty(PlaylistEntryOrder.Renumber([]));
    }
}
