using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class UnitOfWorkSettingExtensionTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static readonly SettingDeclarationInteger Integer = new("SampleInteger", 100, 0, 100);

    private readonly FakeStore _store = new();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private FakeUnitOfWork Uow()
    {
        return new(_store);
    }

    [Fact]
    public async Task GetBoolean_GivesTheSavedValue_OrTheDefaultForNoRowAndForARowOfAnotherType()
    {
        Assert.False(await Uow().GetBoolean(UserID, SettingCatalog.MergeOnRename, Token));

        _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = UserID, Key = SettingKey.MergeOnRename, Value = 1 });
        Assert.False(await Uow().GetBoolean(UserID, SettingCatalog.MergeOnRename, Token));

        _store.UserSettingList.Clear();
        _store.UserSettingList.Add(new EntityUserSettingBoolean { UserID = UserID, Key = SettingKey.MergeOnRename, Value = true });
        Assert.True(await Uow().GetBoolean(UserID, SettingCatalog.MergeOnRename, Token));
    }

    [Fact]
    public async Task GetInteger_GivesTheSavedValue_OrTheDefaultForNoRowAndForANumberOutsideTheLimits()
    {
        Assert.Equal(100, await Uow().GetInteger(UserID, Integer, Token));

        _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = UserID, Key = Integer.Key, Value = 250 });
        Assert.Equal(100, await Uow().GetInteger(UserID, Integer, Token));

        _store.UserSettingList.Clear();
        _store.UserSettingList.Add(new EntityUserSettingInteger { UserID = UserID, Key = Integer.Key, Value = 35 });
        Assert.Equal(35, await Uow().GetInteger(UserID, Integer, Token));
    }

    [Fact]
    public async Task GetEnum_GivesTheMemberAsItsRealType_OrTheDefaultForNoRowAndForAMemberThatWasRemoved()
    {
        Assert.Equal(RepeatModeEnum.No, await Uow().GetEnum(UserID, SettingCatalog.Repeat, Token));

        _store.UserSettingList.Add(new EntityUserSettingEnum { UserID = UserID, Key = SettingKey.Repeat, Value = "Removed" });
        Assert.Equal(RepeatModeEnum.No, await Uow().GetEnum(UserID, SettingCatalog.Repeat, Token));

        _store.UserSettingList.Clear();
        _store.UserSettingList.Add(new EntityUserSettingEnum { UserID = UserID, Key = SettingKey.Repeat, Value = "Forever" });
        Assert.Equal(RepeatModeEnum.Forever, await Uow().GetEnum(UserID, SettingCatalog.Repeat, Token));
    }

    [Fact]
    public async Task TheGetters_NeverReadAnotherUsersRow()
    {
        _store.UserSettingList.Add(new EntityUserSettingBoolean { UserID = Guid.CreateVersion7(), Key = SettingKey.MergeOnRename, Value = true });

        Assert.False(await Uow().GetBoolean(UserID, SettingCatalog.MergeOnRename, Token));
    }
}
