using Noxy.NET.MediaPlayer.Application.Handlers.Metadata;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerSettingsTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakeStore _store = new();
    private readonly HandlerSettings _handler;

    public HandlerSettingsTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<SettingsView> Get(string? ifNoneMatch = null, Guid? userID = null)
    {
        return _handler.Handle(new(userID ?? UserID, ifNoneMatch), Token).AsTask();
    }

    [Fact]
    public async Task Handle_WithNoRow_ReturnsTheDefaultsAndCreatesNothing()
    {
        SettingsView view = await Get();

        Assert.NotNull(view.Settings);
        Assert.False(view.Settings.AutoMergeOnRename);
        Assert.Equal(DateTimeKind.Utc, view.Settings.TimeModified.Kind);
        Assert.Empty(_store.UserSettingsList);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Handle_WithARow_ReturnsIt()
    {
        _store.UserSettingsList.Add(new() { UserID = UserID, AutoMergeOnRename = true });

        SettingsView view = await Get();

        Assert.True(view.Settings?.AutoMergeOnRename);
    }

    [Fact]
    public async Task Handle_TheETagIsStrongAndChangesWhenTheRowChanges()
    {
        string noRow = (await Get()).ETag;
        EntityUserSettings row = new() { UserID = UserID, TimeModified = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        _store.UserSettingsList.Add(row);
        string created = (await Get()).ETag;
        row.TimeModified = row.TimeModified.AddSeconds(1);
        string changed = (await Get()).ETag;

        Assert.Matches("^\"[0-9a-f]{64}\"$", noRow);
        Assert.NotEqual(noRow, created);
        Assert.NotEqual(created, changed);
    }

    [Fact]
    public async Task Handle_AMatchingIfNoneMatch_ReturnsNoSettings_ForTheDefaultsToo()
    {
        string noRow = (await Get()).ETag;
        Assert.Null((await Get(noRow)).Settings);

        _store.UserSettingsList.Add(new() { UserID = UserID });
        string withRow = (await Get()).ETag;
        SettingsView view = await Get(withRow);

        Assert.Null(view.Settings);
        Assert.Equal(withRow, view.ETag);
    }

    [Fact]
    public async Task Handle_AStaleIfNoneMatch_ReturnsTheSettings()
    {
        string noRow = (await Get()).ETag;
        _store.UserSettingsList.Add(new() { UserID = UserID, AutoMergeOnRename = true });

        SettingsView view = await Get(noRow);

        Assert.True(view.Settings?.AutoMergeOnRename);
    }

    [Fact]
    public async Task Handle_AnotherUsersSettings_AreNeverReturned()
    {
        _store.UserSettingsList.Add(new() { UserID = Guid.CreateVersion7(), AutoMergeOnRename = true });

        SettingsView view = await Get();

        Assert.False(view.Settings?.AutoMergeOnRename);
    }
}
