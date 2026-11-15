using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class PlaylistRepositoryTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private async Task<User> CreateUser(string email = "user@example.com")
    {
        User user = new() { Email = email };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<MediaFile> CreateMediaFile(Guid userID)
    {
        MediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 1234,
            ContentHash = new string('a', 64),
            UserID = userID
        };

        file.Metadata = new() { Title = "Media", MediaFileID = file.ID };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Media.AddMediaFile(file);
        await uow.Commit(TestContext.Current.CancellationToken);

        return file;
    }

    private static Playlist CreatePlaylist(Guid userID, string name = "Playlist", DateTime? timeCreated = null)
    {
        return new()
        {
            UserID = userID,
            Name = name,
            TimeCreated = timeCreated ?? DateTime.UtcNow
        };
    }

    private static PlaylistEntry CreateEntry(Playlist playlist, MediaFile mediaFile, int position)
    {
        return new()
        {
            PlaylistID = playlist.ID,
            MediaFileID = mediaFile.ID,
            Position = position
        };
    }

    private async Task Add(Playlist playlist)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Playlist.AddPlaylist(playlist);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddPlaylist_ThenFind_ReturnsItWithoutEntries()
    {
        User user = await CreateUser();
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID, "Road trip");
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        Playlist? found = await uow.Playlist.FindPlaylist(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal("Road trip", found.Name);
        Assert.True(found.AllowDuplicates);
        Assert.Empty(found.EntryList);
    }

    [Fact]
    public async Task AddPlaylist_WithEntries_FindWithEntriesReturnsThemByPosition()
    {
        User user = await CreateUser();
        MediaFile first = await CreateMediaFile(user.ID);
        MediaFile second = await CreateMediaFile(user.ID);
        MediaFile third = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, third, 2));
        playlist.EntryList.Add(CreateEntry(playlist, first, 0));
        playlist.EntryList.Add(CreateEntry(playlist, second, 1));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        Playlist? found = await uow.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal([first.ID, second.ID, third.ID], found.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2], found.EntryList.Select(x => x.Position));
    }

    [Fact]
    public async Task FindPlaylist_AnotherUsersPlaylist_ReturnsNull()
    {
        User owner = await CreateUser("owner@example.com");
        User other = await CreateUser("other@example.com");
        Playlist playlist = CreatePlaylist(owner.ID);
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Playlist.FindPlaylist(playlist.ID, other.ID, TestContext.Current.CancellationToken));
        Assert.Null(await uow.Playlist.FindPlaylistWithEntries(playlist.ID, other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetPlaylistSummaryList_ReturnsOnlyTheUsersPlaylistsNewestFirstWithEntryCounts()
    {
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        DateTime now = DateTime.UtcNow;
        Playlist older = CreatePlaylist(user.ID, "Older", now.AddMinutes(-5));
        older.EntryList.Add(CreateEntry(older, mediaFile, 0));
        older.EntryList.Add(CreateEntry(older, mediaFile, 1));
        Playlist newer = CreatePlaylist(user.ID, "Newer", now);
        await Add(older);
        await Add(newer);
        await Add(CreatePlaylist(other.ID, "Not mine"));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<PlaylistSummary> list = await uow.Playlist.GetPlaylistSummaryList(user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(["Newer", "Older"], list.Select(x => x.Playlist.Name));
        Assert.Equal([0, 2], list.Select(x => x.EntryCount));
        Assert.All(list, x => Assert.Empty(x.Playlist.EntryList));
    }

    [Fact]
    public async Task GetPlaylistStampList_ReturnsIdAndModifiedTimeOfOnlyTheUsersPlaylists()
    {
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        Playlist mine = CreatePlaylist(user.ID);
        mine.TimeModified = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);
        await Add(mine);
        await Add(CreatePlaylist(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<PlaylistStamp> list = await uow.Playlist.GetPlaylistStampList(user.ID, TestContext.Current.CancellationToken);

        PlaylistStamp stamp = Assert.Single(list);
        Assert.Equal(mine.ID, stamp.ID);
        Assert.Equal(mine.TimeModified, stamp.TimeModified);
    }

    [Fact]
    public async Task CountPlaylists_CountsOnlyTheUsersPlaylists()
    {
        User user = await CreateUser();
        User other = await CreateUser("other@example.com");
        await Add(CreatePlaylist(user.ID));
        await Add(CreatePlaylist(user.ID));
        await Add(CreatePlaylist(other.ID));

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(2, await uow.Playlist.CountPlaylists(user.ID, TestContext.Current.CancellationToken));
        Assert.Equal(1, await uow.Playlist.CountPlaylists(other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdatePlaylist_SavesNameFlagAndModifiedTime_AndLeavesTheEntriesAlone()
    {
        User user = await CreateUser();
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID, "Before");
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);
        DateTime modified = new(2026, 5, 6, 7, 8, 9, DateTimeKind.Utc);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            Playlist loaded = await uow.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken)
                              ?? throw new InvalidOperationException("The playlist was not found.");
            loaded.Name = "After";
            loaded.AllowDuplicates = false;
            loaded.TimeModified = modified;
            uow.Playlist.UpdatePlaylist(loaded);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Playlist? found = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal("After", found.Name);
        Assert.False(found.AllowDuplicates);
        Assert.Equal(modified, found.TimeModified);
        Assert.Single(found.EntryList);
    }

    [Fact]
    public async Task UpdatePlaylistEntry_SavesAPositionChange()
    {
        User user = await CreateUser();
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID);
        PlaylistEntry entry = CreateEntry(playlist, mediaFile, 0);
        playlist.EntryList.Add(entry);
        await Add(playlist);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            entry.Position = 7;
            uow.Playlist.UpdatePlaylistEntry(entry);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Playlist? found = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal(7, Assert.Single(found.EntryList).Position);
    }

    [Fact]
    public async Task AddRemoveAndUpdateEntries_AreAppliedInOneCommit()
    {
        User user = await CreateUser();
        MediaFile first = await CreateMediaFile(user.ID);
        MediaFile second = await CreateMediaFile(user.ID);
        MediaFile third = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID);
        PlaylistEntry keep = CreateEntry(playlist, first, 0);
        PlaylistEntry drop = CreateEntry(playlist, second, 1);
        playlist.EntryList.Add(keep);
        playlist.EntryList.Add(drop);
        await Add(playlist);
        PlaylistEntry added = CreateEntry(playlist, third, 0);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Playlist.RemovePlaylistEntry(drop);
            uow.Playlist.AddPlaylistEntry(added);
            keep.Position = 1;
            uow.Playlist.UpdatePlaylistEntry(keep);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Playlist? found = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal([added.ID, keep.ID], found.EntryList.Select(x => x.ID));
        Assert.Equal([0, 1], found.EntryList.Select(x => x.Position));
    }

    [Fact]
    public async Task UpdatePlaylistEntry_ManyPositionsAtOnce_DoesNotViolateAnyConstraint()
    {
        User user = await CreateUser();
        MediaFile first = await CreateMediaFile(user.ID);
        MediaFile second = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID);
        PlaylistEntry a = CreateEntry(playlist, first, 0);
        PlaylistEntry b = CreateEntry(playlist, second, 1);
        playlist.EntryList.Add(a);
        playlist.EntryList.Add(b);
        await Add(playlist);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            a.Position = 1;
            b.Position = 0;
            uow.Playlist.UpdatePlaylistEntry(a);
            uow.Playlist.UpdatePlaylistEntry(b);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Playlist? found = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal([b.ID, a.ID], found.EntryList.Select(x => x.ID));
    }

    [Fact]
    public async Task RemovePlaylist_DeletesItsEntriesButNotTheMediaFiles()
    {
        User user = await CreateUser();
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Playlist.RemovePlaylist(playlist);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using DataContext context = _fixture.CreateContext();

        Assert.Empty(await context.Playlist.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await context.PlaylistEntry.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Single(await context.MediaFile.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RemoveMediaFile_DeletesItsEntriesFromEveryPlaylist()
    {
        User user = await CreateUser();
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        MediaFile other = await CreateMediaFile(user.ID);
        Playlist one = CreatePlaylist(user.ID, "One");
        one.EntryList.Add(CreateEntry(one, mediaFile, 0));
        one.EntryList.Add(CreateEntry(one, other, 1));
        Playlist two = CreatePlaylist(user.ID, "Two");
        two.EntryList.Add(CreateEntry(two, mediaFile, 0));
        await Add(one);
        await Add(two);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Media.RemoveMediaFile(mediaFile);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using DataContext context = _fixture.CreateContext();
        var left = await context.PlaylistEntry.ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(other.ID, Assert.Single(left).MediaFileID);
    }

    [Fact]
    public async Task GetPlaylistListByMediaFileID_ReturnsOnlyThePlaylistsContainingTheMediaFileWithAllTheirEntries()
    {
        User user = await CreateUser();
        User stranger = await CreateUser("stranger@example.com");
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        MediaFile other = await CreateMediaFile(user.ID);
        Playlist holds = CreatePlaylist(user.ID, "Holds");
        holds.EntryList.Add(CreateEntry(holds, other, 0));
        holds.EntryList.Add(CreateEntry(holds, mediaFile, 1));
        Playlist without = CreatePlaylist(user.ID, "Without");
        without.EntryList.Add(CreateEntry(without, other, 0));
        Playlist foreign = CreatePlaylist(stranger.ID, "Foreign");
        foreign.EntryList.Add(CreateEntry(foreign, mediaFile, 0));
        await Add(holds);
        await Add(without);
        await Add(foreign);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<Playlist> list = await uow.Playlist.GetPlaylistListByMediaFileID(user.ID, mediaFile.ID, TestContext.Current.CancellationToken);

        Playlist found = Assert.Single(list);
        Assert.Equal("Holds", found.Name);
        Assert.Equal([other.ID, mediaFile.ID], found.EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task SameMediaFileTwiceInOnePlaylist_IsAllowed()
    {
        User user = await CreateUser();
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 1));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        Playlist? found = await uow.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal(2, found.EntryList.Count);
    }

    [Fact]
    public async Task FindPlaylistWithEntries_LoadsEachEntrysMediaFileWithMetadata()
    {
        User user = await CreateUser();
        MediaFile mediaFile = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        Playlist? found = await uow.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        MediaFile loaded = Assert.IsType<MediaFile>(Assert.Single(found.EntryList).MediaFile);
        Assert.Equal(mediaFile.ID, loaded.ID);
        Assert.Equal(mediaFile.ContentHash, loaded.ContentHash);
        Assert.Equal("Media", loaded.Metadata?.Title);
    }

    [Fact]
    public async Task GetEntryMediaFileStampList_ReturnsTheMediaFileIdAndModifiedTimeForEveryEntry_IncludingRepeatedMediaFiles()
    {
        User user = await CreateUser();
        MediaFile first = await CreateMediaFile(user.ID);
        MediaFile second = await CreateMediaFile(user.ID);
        Playlist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, first, 0));
        playlist.EntryList.Add(CreateEntry(playlist, second, 1));
        playlist.EntryList.Add(CreateEntry(playlist, first, 2));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<PlaylistStamp> stampList = await uow.Playlist.GetEntryMediaFileStampList(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(3, stampList.Count);
        Assert.Equal(2, stampList.Count(x => x.ID == first.ID));
        Assert.Equal(first.TimeModified, stampList.First(x => x.ID == first.ID).TimeModified);
        Assert.Equal(second.TimeModified, stampList.Single(x => x.ID == second.ID).TimeModified);
    }

    [Fact]
    public async Task GetEntryMediaFileStampList_AnotherUsersPlaylist_ReturnsEmpty()
    {
        User owner = await CreateUser("owner@example.com");
        User other = await CreateUser("other@example.com");
        MediaFile mediaFile = await CreateMediaFile(owner.ID);
        Playlist playlist = CreatePlaylist(owner.ID);
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Empty(await uow.Playlist.GetEntryMediaFileStampList(playlist.ID, other.ID, TestContext.Current.CancellationToken));
    }
}
