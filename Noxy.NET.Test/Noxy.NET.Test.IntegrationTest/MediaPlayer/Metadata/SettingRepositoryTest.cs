using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class SettingRepositoryTest : IDisposable
{
    // The repository keeps any key it is given; this one is no longer a declared setting.
    private const string SampleKey = "SampleInteger";

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

    private async Task Add(EntityUserSetting setting)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        uow.Setting.AddSetting(setting);
        await uow.Commit(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EachValueType_IsAddedAndReadBackAsItsOwnDerivedTypeWithItsValue()
    {
        EntityUser user = await CreateUser();
        await Add(new EntityUserSettingBoolean { UserID = user.ID, Key = SettingKey.MergeOnRename, Value = true });
        await Add(new EntityUserSettingInteger { UserID = user.ID, Key = SampleKey, Value = 40 });
        await Add(new EntityUserSettingEnum { UserID = user.ID, Key = SettingKey.Repeat, Value = "Forever" });

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        List<EntityUserSetting> list = await uow.Setting.GetSettingList(user.ID, TestContext.Current.CancellationToken);

        Assert.Equal(3, list.Count);
        Assert.True(Assert.IsType<EntityUserSettingBoolean>(list.Single(x => x.Key == SettingKey.MergeOnRename)).Value);
        Assert.Equal(40, Assert.IsType<EntityUserSettingInteger>(list.Single(x => x.Key == SampleKey)).Value);
        Assert.Equal("Forever", Assert.IsType<EntityUserSettingEnum>(list.Single(x => x.Key == SettingKey.Repeat)).Value);
    }

    [Fact]
    public async Task FindSetting_FindsByUserAndKey_AndNotAnotherUsersRowOrAnUnknownKey()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        await Add(new EntityUserSettingInteger { UserID = owner.ID, Key = SampleKey, Value = 10 });

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        EntityUserSettingInteger found = Assert.IsType<EntityUserSettingInteger>(await uow.Setting.FindSetting(owner.ID, SampleKey, TestContext.Current.CancellationToken));
        Assert.Equal(10, found.Value);
        Assert.Null(await uow.Setting.FindSetting(other.ID, SampleKey, TestContext.Current.CancellationToken));
        Assert.Null(await uow.Setting.FindSetting(owner.ID, "Nope", TestContext.Current.CancellationToken));
        Assert.Null(await uow.Setting.FindSetting(owner.ID, "volume", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetSettingList_HoldsOnlyTheUsersRows()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        await Add(new EntityUserSettingInteger { UserID = owner.ID, Key = SampleKey, Value = 10 });
        await Add(new EntityUserSettingInteger { UserID = other.ID, Key = SampleKey, Value = 99 });

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Equal(10, Assert.IsType<EntityUserSettingInteger>(Assert.Single(await uow.Setting.GetSettingList(owner.ID, TestContext.Current.CancellationToken))).Value);
        Assert.Empty(await uow.Setting.GetSettingList(Guid.CreateVersion7(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ASecondRowWithTheSameUserAndKey_ThrowsMetadataKeyConflict_ButTwoUsersMayShareAKey()
    {
        EntityUser first = await CreateUser("first@example.com");
        EntityUser second = await CreateUser("second@example.com");
        await Add(new EntityUserSettingInteger { UserID = first.ID, Key = SampleKey, Value = 10 });
        await Add(new EntityUserSettingInteger { UserID = second.ID, Key = SampleKey, Value = 20 });

        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => Add(new EntityUserSettingInteger { UserID = first.ID, Key = SampleKey, Value = 30 }));
    }

    [Fact]
    public async Task ARowOfAnotherTypeForTheSameKey_IsAlsoAConflict()
    {
        EntityUser user = await CreateUser();
        await Add(new EntityUserSettingInteger { UserID = user.ID, Key = SampleKey, Value = 10 });

        await Assert.ThrowsAsync<MetadataKeyConflictException>(() => Add(new EntityUserSettingBoolean { UserID = user.ID, Key = SampleKey, Value = true }));
    }

    [Fact]
    public async Task UpdateSetting_SavesTheChangedValueAndModifiedTime()
    {
        EntityUser user = await CreateUser();
        EntityUserSettingInteger setting = new() { UserID = user.ID, Key = SampleKey, Value = 10, TimeModified = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        await Add(setting);

        setting.Value = 55;
        setting.TimeModified = new(2027, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Setting.UpdateSetting(setting);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork read = _fixture.UoWFactory.Create();
        EntityUserSettingInteger found = Assert.IsType<EntityUserSettingInteger>(await read.Setting.FindSetting(user.ID, SampleKey, TestContext.Current.CancellationToken));
        Assert.Equal(55, found.Value);
        Assert.Equal(new DateTime(2027, 2, 3, 4, 5, 6), found.TimeModified);
    }

    [Fact]
    public async Task RemovingAUser_RemovesTheirSettings()
    {
        EntityUser user = await CreateUser();
        await Add(new EntityUserSettingBoolean { UserID = user.ID, Key = SettingKey.MergeOnRename, Value = true });
        await Add(new EntityUserSettingEnum { UserID = user.ID, Key = SettingKey.Shuffle, Value = "Random" });

        await using (DataContext context = _fixture.CreateContext())
        {
            await context.User.Where(x => x.ID == user.ID).ExecuteDeleteAsync(TestContext.Current.CancellationToken);
        }

        await using DataContext check = _fixture.CreateContext();
        Assert.Equal(0, await check.UserSetting.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await check.UserSettingBoolean.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await check.UserSettingEnum.CountAsync(TestContext.Current.CancellationToken));
    }
}
