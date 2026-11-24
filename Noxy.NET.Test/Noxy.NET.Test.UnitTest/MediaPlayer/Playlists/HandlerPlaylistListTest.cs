using Noxy.NET.MediaPlayer.Application.Handlers.Playlist;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class HandlerPlaylistListTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly HandlerPlaylistList _handler;
    private DateTime _now = Start;

    public HandlerPlaylistListTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void Advance()
    {
        _now = _now.AddSeconds(1);
    }

    private EntityMediaFile MediaFile()
    {
        EntityMediaFile file = new()
        {
            FilePath = $"{Guid.CreateVersion7()}.mp3",
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            ContentHash = new('a', 64),
            UserID = UserID,
            TimeModified = _now
        };

        _store.MediaFileList.Add(file);
        return file;
    }

    private EntityPlaylist Seed(string name = "Mix", bool allowDuplicates = true, Guid? userID = null, int entryCount = 0, int position = 0)
    {
        EntityPlaylist playlist = new()
        {
            UserID = userID ?? UserID,
            Name = name,
            AllowDuplicates = allowDuplicates,
            Position = position,
            TimeCreated = _now,
            TimeModified = _now
        };

        for (int index = 0; index < entryCount; index++)
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = MediaFile().ID, Position = index });
        }

        _store.PlaylistList.Add(playlist);
        return playlist;
    }

    private async Task<PlaylistLibrary> Library(string? ifNoneMatch = null, Guid? userID = null)
    {
        return await _handler.Handle(new(userID ?? UserID, ifNoneMatch), Token);
    }

    [Fact]
    public async Task Handle_ReturnsTheUsersPlaylistsWithEntryCountsAndNoEntries()
    {
        EntityPlaylist first = Seed("First", entryCount: 3);
        Advance();
        EntityPlaylist second = Seed("Second");
        Seed("Not mine", userID: Guid.CreateVersion7());

        PlaylistLibrary library = await Library();

        Assert.Matches("^\"[0-9a-f]{64}\"$", library.ETag);
        Assert.Equal(2, library.PlaylistList?.Count);
        Assert.Equal(3, library.PlaylistList?.Single(x => x.ID == first.ID).EntryCount);
        Assert.Equal(0, library.PlaylistList?.Single(x => x.ID == second.ID).EntryCount);
        Assert.All(library.PlaylistList ?? [], x => Assert.IsNotType<ResponsePlaylistDetail>(x));
    }

    [Fact]
    public async Task Handle_IsInPositionOrder_WhateverWasCreatedFirst()
    {
        Seed("Second", position: 1);
        Advance();
        Seed("First", position: 0);
        Advance();
        Seed("Third", position: 2);

        PlaylistLibrary library = await Library();

        Assert.Equal(["First", "Second", "Third"], library.PlaylistList?.Select(x => x.Name) ?? []);
    }

    [Fact]
    public async Task Handle_ETagChanges_WhenOnlyAPositionChanges()
    {
        EntityPlaylist first = Seed("First", position: 0);
        Seed("Second", position: 1);
        string before = (await Library()).ETag;

        first.Position = 1;

        Assert.NotEqual(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Handle_ETagChangesAfterEveryKindOfChange()
    {
        EntityPlaylist playlist = Seed();
        List<string> eTagList = [(await Library()).ETag];

        async Task Step(Action change)
        {
            Advance();
            change();
            eTagList.Add((await Library()).ETag);
        }

        EntityPlaylist? other = null;
        await Step(() =>
        {
            playlist.Name = "Renamed";
            playlist.TimeModified = _now;
        });
        await Step(() =>
        {
            playlist.AllowDuplicates = false;
            playlist.TimeModified = _now;
        });
        await Step(() =>
        {
            playlist.EntryList.Add(new() { PlaylistID = playlist.ID, MediaFileID = MediaFile().ID, Position = 0 });
            playlist.TimeModified = _now;
        });
        await Step(() => other = Seed("Second"));
        await Step(() => _store.PlaylistList.RemoveAll(x => x.ID == other?.ID));

        for (int index = 1; index < eTagList.Count; index++)
        {
            Assert.NotEqual(eTagList[index - 1], eTagList[index]);
        }
    }

    [Fact]
    public async Task Handle_ETagChangesWhenOnlyTheLastPlayedTimeChanges_AndAnOldTagGetsTheNewList()
    {
        EntityPlaylist playlist = Seed();
        string neverPlayed = (await Library()).ETag;
        Advance();

        playlist.TimeLastPlayed = _now;
        PlaylistLibrary played = await Library();
        Assert.NotEqual(neverPlayed, played.ETag);
        Assert.Equal(_now, played.PlaylistList?.Single().TimeLastPlayed);

        PlaylistLibrary old = await Library(neverPlayed);
        Assert.NotNull(old.PlaylistList);

        PlaylistLibrary current = await Library(played.ETag);
        Assert.Null(current.PlaylistList);

        Advance();
        playlist.TimeLastPlayed = _now;
        Assert.NotEqual(played.ETag, (await Library()).ETag);
    }

    [Fact]
    public async Task Handle_SameState_KeepsTheETag()
    {
        Seed(allowDuplicates: false, entryCount: 2);
        string before = (await Library()).ETag;
        Advance();

        Assert.Equal(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Handle_MatchingIfNoneMatch_ReturnsNoList()
    {
        Seed();
        string eTag = (await Library()).ETag;

        PlaylistLibrary matched = await Library(eTag);
        PlaylistLibrary wildcard = await Library("*");

        Assert.Equal(eTag, matched.ETag);
        Assert.Null(matched.PlaylistList);
        Assert.Null(wildcard.PlaylistList);
    }

    [Theory]
    [InlineData("\"other\"")]
    [InlineData("garbage")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Handle_MismatchedIfNoneMatch_ReturnsTheList(string? ifNoneMatch)
    {
        Seed();

        PlaylistLibrary library = await Library(ifNoneMatch);

        Assert.Equal(1, library.PlaylistList?.Count);
    }

    [Fact]
    public async Task Handle_IsPerUser()
    {
        Seed();
        string before = (await Library()).ETag;

        Seed(userID: Guid.CreateVersion7());

        Assert.Equal(before, (await Library()).ETag);
    }

    [Fact]
    public async Task Handle_EmptyList_HasAStableETag()
    {
        PlaylistLibrary first = await Library();
        PlaylistLibrary second = await Library();

        Assert.Equal(first.ETag, second.ETag);
        Assert.NotNull(first.PlaylistList);
        Assert.Empty(first.PlaylistList);
    }

    [Fact]
    public async Task Handle_ETagIsUnchangedByAMediaFileEdit()
    {
        EntityPlaylist playlist = Seed(entryCount: 1);
        string before = (await Library()).ETag;
        Advance();
        _store.MediaFileList.Single(x => x.ID == playlist.EntryList[0].MediaFileID).TimeModified = _now;

        Assert.Equal(before, (await Library()).ETag);
    }
}
