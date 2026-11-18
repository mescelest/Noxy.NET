using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.MediaPlayer.Persistence.Tables.Playlist;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class PlaylistRepositoryTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private async Task<EntityUser> CreateUser(string email = "user@example.com")
    {
        EntityUser user = new() { Email = email };

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Authentication.AddUser(user);
        await uow.Commit(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<EntityMediaFile> CreateMediaFile(Guid userID)
    {
        EntityMediaFile file = new()
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

    private static EntityPlaylist CreatePlaylist(Guid userID, string name = "Playlist", DateTime? timeCreated = null)
    {
        return new()
        {
            UserID = userID,
            Name = name,
            TimeCreated = timeCreated ?? DateTime.UtcNow
        };
    }

    private static EntityPlaylistEntry CreateEntry(EntityPlaylist playlist, EntityMediaFile mediaFile, int position)
    {
        return new()
        {
            PlaylistID = playlist.ID,
            MediaFileID = mediaFile.ID,
            Position = position
        };
    }

    private async Task Add(EntityPlaylist playlist)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Playlist.AddPlaylist(playlist);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddPlaylist_ThenFind_ReturnsItWithoutEntries()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID, "Road trip");
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityPlaylist? found = await uow.Playlist.FindPlaylist(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal("Road trip", found.Name);
        Assert.True(found.AllowDuplicates);
        Assert.Empty(found.EntryList);
    }

    [Fact]
    public async Task AddPlaylist_WithEntries_FindWithEntriesReturnsThemByPosition()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile first = await CreateMediaFile(user.ID);
        EntityMediaFile second = await CreateMediaFile(user.ID);
        EntityMediaFile third = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, third, 2));
        playlist.EntryList.Add(CreateEntry(playlist, first, 0));
        playlist.EntryList.Add(CreateEntry(playlist, second, 1));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityPlaylist? found = await uow.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal([first.ID, second.ID, third.ID], found.EntryList.Select(x => x.MediaFileID));
        Assert.Equal([0, 1, 2], found.EntryList.Select(x => x.Position));
    }

    [Fact]
    public async Task FindPlaylist_AnotherUsersPlaylist_ReturnsNull()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        EntityPlaylist playlist = CreatePlaylist(owner.ID);
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Playlist.FindPlaylist(playlist.ID, other.ID, TestContext.Current.CancellationToken));
        Assert.Null(await uow.Playlist.FindPlaylistWithEntries(playlist.ID, other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetPlaylistSummaryList_ReturnsOnlyTheUsersPlaylistsNewestFirstWithEntryCounts()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        DateTime now = DateTime.UtcNow;
        EntityPlaylist older = CreatePlaylist(user.ID, "Older", now.AddMinutes(-5));
        older.EntryList.Add(CreateEntry(older, mediaFile, 0));
        older.EntryList.Add(CreateEntry(older, mediaFile, 1));
        EntityPlaylist newer = CreatePlaylist(user.ID, "Newer", now);
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
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        EntityPlaylist mine = CreatePlaylist(user.ID);
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
    public async Task TimeLastPlayed_IsNullByDefault_ThenSavedAndReturnedByFindSummaryAndStamp_WithoutTouchingTheEntries()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile file = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, file, 0));
        await Add(playlist);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            Assert.Null((await uow.Playlist.FindPlaylist(playlist.ID, user.ID, TestContext.Current.CancellationToken))?.TimeLastPlayed);
            Assert.Null((await uow.Playlist.GetPlaylistStampList(user.ID, TestContext.Current.CancellationToken)).Single().TimeLastPlayed);
        }

        DateTime played = new(2026, 7, 8, 9, 10, 11, DateTimeKind.Utc);
        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            EntityPlaylist found = await uow.Playlist.FindPlaylist(playlist.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The playlist was not saved.");
            found.TimeLastPlayed = played;
            uow.Playlist.UpdatePlaylist(found);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        Assert.Equal(played, (await check.Playlist.FindPlaylist(playlist.ID, user.ID, TestContext.Current.CancellationToken))?.TimeLastPlayed);
        Assert.Equal(played, (await check.Playlist.GetPlaylistStampList(user.ID, TestContext.Current.CancellationToken)).Single().TimeLastPlayed);
        Assert.Equal(played, (await check.Playlist.GetPlaylistSummaryList(user.ID, TestContext.Current.CancellationToken)).Single().Playlist.TimeLastPlayed);
        EntityPlaylist withEntries = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken) ?? throw new InvalidOperationException("The playlist was not saved.");
        Assert.Single(withEntries.EntryList);
    }

    [Fact]
    public async Task FindPlaylistSummary_ReturnsThePlaylistWithItsEntryCountAndNoEntries_AndNullForAnotherUsersOrUnknown()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaFile first = await CreateMediaFile(user.ID);
        EntityMediaFile second = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, first, 0));
        playlist.EntryList.Add(CreateEntry(playlist, second, 1));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        PlaylistSummary? found = await uow.Playlist.FindPlaylistSummary(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal(2, found.EntryCount);
        Assert.Equal(playlist.ID, found.Playlist.ID);
        Assert.Empty(found.Playlist.EntryList);
        Assert.Null(await uow.Playlist.FindPlaylistSummary(playlist.ID, other.ID, TestContext.Current.CancellationToken));
        Assert.Null(await uow.Playlist.FindPlaylistSummary(Guid.CreateVersion7(), user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CountPlaylists_CountsOnlyTheUsersPlaylists()
    {
        EntityUser user = await CreateUser();
        EntityUser other = await CreateUser("other@example.com");
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
        EntityUser user = await CreateUser();
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID, "Before");
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);
        DateTime modified = new(2026, 5, 6, 7, 8, 9, DateTimeKind.Utc);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            EntityPlaylist loaded = await uow.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken)
                                    ?? throw new InvalidOperationException("The playlist was not found.");
            loaded.Name = "After";
            loaded.AllowDuplicates = false;
            loaded.TimeModified = modified;
            uow.Playlist.UpdatePlaylist(loaded);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        EntityPlaylist? found = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal("After", found.Name);
        Assert.False(found.AllowDuplicates);
        Assert.Equal(modified, found.TimeModified);
        Assert.Single(found.EntryList);
    }

    [Fact]
    public async Task UpdatePlaylistEntry_SavesAPositionChange()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
        EntityPlaylistEntry entry = CreateEntry(playlist, mediaFile, 0);
        playlist.EntryList.Add(entry);
        await Add(playlist);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            entry.Position = 7;
            uow.Playlist.UpdatePlaylistEntry(entry);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        EntityPlaylist? found = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal(7, Assert.Single(found.EntryList).Position);
    }

    [Fact]
    public async Task AddRemoveAndUpdateEntries_AreAppliedInOneCommit()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile first = await CreateMediaFile(user.ID);
        EntityMediaFile second = await CreateMediaFile(user.ID);
        EntityMediaFile third = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
        EntityPlaylistEntry keep = CreateEntry(playlist, first, 0);
        EntityPlaylistEntry drop = CreateEntry(playlist, second, 1);
        playlist.EntryList.Add(keep);
        playlist.EntryList.Add(drop);
        await Add(playlist);
        EntityPlaylistEntry added = CreateEntry(playlist, third, 0);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Playlist.RemovePlaylistEntry(drop);
            uow.Playlist.AddPlaylistEntry(added);
            keep.Position = 1;
            uow.Playlist.UpdatePlaylistEntry(keep);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork check = _fixture.UoWFactory.Create();
        EntityPlaylist? found = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal([added.ID, keep.ID], found.EntryList.Select(x => x.ID));
        Assert.Equal([0, 1], found.EntryList.Select(x => x.Position));
    }

    [Fact]
    public async Task UpdatePlaylistEntry_ManyPositionsAtOnce_DoesNotViolateAnyConstraint()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile first = await CreateMediaFile(user.ID);
        EntityMediaFile second = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
        EntityPlaylistEntry a = CreateEntry(playlist, first, 0);
        EntityPlaylistEntry b = CreateEntry(playlist, second, 1);
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
        EntityPlaylist? found = await check.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal([b.ID, a.ID], found.EntryList.Select(x => x.ID));
    }

    [Fact]
    public async Task RemovePlaylist_DeletesItsEntriesButNotTheMediaFiles()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
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
        EntityUser user = await CreateUser();
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        EntityMediaFile other = await CreateMediaFile(user.ID);
        EntityPlaylist one = CreatePlaylist(user.ID, "One");
        one.EntryList.Add(CreateEntry(one, mediaFile, 0));
        one.EntryList.Add(CreateEntry(one, other, 1));
        EntityPlaylist two = CreatePlaylist(user.ID, "Two");
        two.EntryList.Add(CreateEntry(two, mediaFile, 0));
        await Add(one);
        await Add(two);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Media.RemoveMediaFile(mediaFile);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using DataContext context = _fixture.CreateContext();
        List<TablePlaylistEntry> left = await context.PlaylistEntry.ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(other.ID, Assert.Single(left).MediaFileID);
    }

    [Fact]
    public async Task GetPlaylistListByMediaFileID_ReturnsOnlyThePlaylistsContainingTheMediaFileWithAllTheirEntries()
    {
        EntityUser user = await CreateUser();
        EntityUser stranger = await CreateUser("stranger@example.com");
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        EntityMediaFile other = await CreateMediaFile(user.ID);
        EntityPlaylist holds = CreatePlaylist(user.ID, "Holds");
        holds.EntryList.Add(CreateEntry(holds, other, 0));
        holds.EntryList.Add(CreateEntry(holds, mediaFile, 1));
        EntityPlaylist without = CreatePlaylist(user.ID, "Without");
        without.EntryList.Add(CreateEntry(without, other, 0));
        EntityPlaylist foreign = CreatePlaylist(stranger.ID, "Foreign");
        foreign.EntryList.Add(CreateEntry(foreign, mediaFile, 0));
        await Add(holds);
        await Add(without);
        await Add(foreign);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<EntityPlaylist> list = await uow.Playlist.GetPlaylistListByMediaFileID(user.ID, mediaFile.ID, TestContext.Current.CancellationToken);

        EntityPlaylist found = Assert.Single(list);
        Assert.Equal("Holds", found.Name);
        Assert.Equal([other.ID, mediaFile.ID], found.EntryList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task SameMediaFileTwiceInOnePlaylist_IsAllowed()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 1));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityPlaylist? found = await uow.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal(2, found.EntryList.Count);
    }

    [Fact]
    public async Task FindPlaylistWithEntries_LoadsEachEntrysMediaFileWithMetadata()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile mediaFile = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        EntityPlaylist? found = await uow.Playlist.FindPlaylistWithEntries(playlist.ID, user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        EntityMediaFile loaded = Assert.IsType<EntityMediaFile>(Assert.Single(found.EntryList).MediaFile);
        Assert.Equal(mediaFile.ID, loaded.ID);
        Assert.Equal(mediaFile.ContentHash, loaded.ContentHash);
        Assert.Equal("Media", loaded.Metadata?.Title);
    }

    [Fact]
    public async Task GetEntryMediaFileStampList_ReturnsTheMediaFileIdAndModifiedTimeForEveryEntry_IncludingRepeatedMediaFiles()
    {
        EntityUser user = await CreateUser();
        EntityMediaFile first = await CreateMediaFile(user.ID);
        EntityMediaFile second = await CreateMediaFile(user.ID);
        EntityPlaylist playlist = CreatePlaylist(user.ID);
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
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        EntityMediaFile mediaFile = await CreateMediaFile(owner.ID);
        EntityPlaylist playlist = CreatePlaylist(owner.ID);
        playlist.EntryList.Add(CreateEntry(playlist, mediaFile, 0));
        await Add(playlist);

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Empty(await uow.Playlist.GetEntryMediaFileStampList(playlist.ID, other.ID, TestContext.Current.CancellationToken));
    }
}
