using Noxy.NET.MediaPlayer.Application.Handlers.Setting;
using Noxy.NET.MediaPlayer.Application.Models.Setting;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class HandlerSettingListTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly DateTime Saved = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);

    private readonly FakeStore _store = new();
    private readonly HandlerSettingList _handler;

    public HandlerSettingListTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new ETagService());
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<SettingLibrary> List(string? ifNoneMatch = null, Guid? userID = null)
    {
        return _handler.Handle(new(userID ?? UserID, ifNoneMatch), Token).AsTask();
    }

    private EntityUserSettingInteger SeedVolume(int value, Guid? userID = null)
    {
        EntityUserSettingInteger row = new() { UserID = userID ?? UserID, Key = SettingKey.Volume, Value = value, TimeModified = Saved };
        _store.UserSettingList.Add(row);
        return row;
    }

    [Fact]
    public async Task Handle_WithNoRows_ReturnsTheFiveDefaultsInCatalogOrderDatedAtTheEpoch()
    {
        SettingLibrary library = await List();

        Assert.NotNull(library.SettingList);
        Assert.Equal([SettingKey.Shuffle, SettingKey.Repeat, SettingKey.Volume, SettingKey.MergeOnRename, SettingKey.KeepQueuedItems], library.SettingList.Select(x => x.Key).ToList());
        Assert.Equal(["Off", "No", 100, false, false], library.SettingList.Select(x => x.Value).ToList());
        Assert.Equal([SettingValueTypeEnum.Enum, SettingValueTypeEnum.Enum, SettingValueTypeEnum.Integer, SettingValueTypeEnum.Boolean, SettingValueTypeEnum.Boolean], library.SettingList.Select(x => x.ValueType).ToList());
        Assert.All(library.SettingList, x => Assert.Equal(DateTime.UnixEpoch, x.TimeModified));
        Assert.All(library.SettingList, x => Assert.Equal(DateTimeKind.Utc, x.TimeModified.Kind));
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Handle_SavedRowsReplaceTheirDefaults_WithTheirOwnModifiedTime()
    {
        SeedVolume(40);
        _store.UserSettingList.Add(new EntityUserSettingEnum { UserID = UserID, Key = SettingKey.Repeat, Value = "Forever", TimeModified = Saved });
        _store.UserSettingList.Add(new EntityUserSettingBoolean { UserID = UserID, Key = SettingKey.MergeOnRename, Value = true, TimeModified = Saved });

        SettingLibrary library = await List();

        Assert.NotNull(library.SettingList);
        Assert.Equal(["Off", "Forever", 40, true, false], library.SettingList.Select(x => x.Value).ToList());
        Assert.Equal(DateTime.UnixEpoch, library.SettingList.Single(x => x.Key == SettingKey.Shuffle).TimeModified);
        Assert.Equal(Saved, library.SettingList.Single(x => x.Key == SettingKey.Volume).TimeModified);
        Assert.Equal(DateTimeKind.Utc, library.SettingList.Single(x => x.Key == SettingKey.Volume).TimeModified.Kind);
    }

    [Fact]
    public async Task Handle_ARowForAKeyThatIsNoLongerDeclared_IsIgnored()
    {
        _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = UserID, Key = "Removed", Value = 7, TimeModified = Saved });

        SettingLibrary library = await List();

        Assert.NotNull(library.SettingList);
        Assert.Equal(5, library.SettingList.Count);
        Assert.DoesNotContain(library.SettingList, x => x.Key == "Removed");
    }

    [Fact]
    public async Task Handle_ARowOfTheWrongTypeOrAnUnknownMember_IsReadAsTheDefault_AndNeverBreakTheList()
    {
        _store.UserSettingList.Add(new EntityUserSettingBoolean { UserID = UserID, Key = SettingKey.Volume, Value = true, TimeModified = Saved });
        _store.UserSettingList.Add(new EntityUserSettingEnum { UserID = UserID, Key = SettingKey.Repeat, Value = "Removed", TimeModified = Saved });
        _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = UserID, Key = SettingKey.Shuffle, Value = 1, TimeModified = Saved });

        SettingLibrary library = await List();

        Assert.NotNull(library.SettingList);
        Assert.Equal(["Off", "No", 100, false, false], library.SettingList.Select(x => x.Value).ToList());
    }

    [Fact]
    public async Task Handle_TheETagIsStrong_DiffersBetweenNoRowsAndARow_AndChangesWithAValue()
    {
        string none = (await List()).ETag;
        EntityUserSettingInteger row = SeedVolume(40);
        string saved = (await List()).ETag;
        row.TimeModified = Saved.AddSeconds(1);
        string changed = (await List()).ETag;

        Assert.Matches("^\"[0-9a-f]{64}\"$", none);
        Assert.NotEqual(none, saved);
        Assert.NotEqual(saved, changed);
    }

    [Fact]
    public async Task Handle_TheETag_IgnoresAnotherUsersRowsAndRowsForUndeclaredKeys()
    {
        string before = (await List()).ETag;

        SeedVolume(10, Guid.CreateVersion7());
        _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = UserID, Key = "Removed", Value = 7, TimeModified = Saved });

        Assert.Equal(before, (await List()).ETag);
    }

    [Fact]
    public async Task Handle_AMatchingIfNoneMatch_ReturnsNoList_AlsoWhenWeak_AndAStaleOneReturnsIt()
    {
        string none = (await List()).ETag;
        Assert.Null((await List(none)).SettingList);
        Assert.Null((await List("W/" + none)).SettingList);

        SeedVolume(40);
        SettingLibrary fresh = await List(none);

        Assert.NotNull(fresh.SettingList);
        Assert.NotEqual(none, fresh.ETag);
    }

    [Fact]
    public async Task Handle_AnotherUsersSettings_AreNeverReturned()
    {
        SeedVolume(10, Guid.CreateVersion7());

        SettingLibrary library = await List();

        Assert.NotNull(library.SettingList);
        Assert.Equal(100, library.SettingList.Single(x => x.Key == SettingKey.Volume).Value);
    }
}
