using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class SettingsRepositoryTest : IDisposable
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

    [Fact]
    public async Task FindSettings_WithNoRow_ReturnsNull()
    {
        EntityUser user = await CreateUser();

        await using IUnitOfWork uow = _fixture.UoWFactory.Create();

        Assert.Null(await uow.Settings.FindSettings(user.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddSettings_ThenFind_ReturnsIt()
    {
        EntityUser user = await CreateUser();
        EntityUserSettings settings = new() { UserID = user.ID, AutoMergeOnRename = true };

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Settings.AddSettings(settings);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork read = _fixture.UoWFactory.Create();
        EntityUserSettings? found = await read.Settings.FindSettings(user.ID, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.True(found.AutoMergeOnRename);
    }

    [Fact]
    public async Task UpdateSettings_SavesTheChange()
    {
        EntityUser user = await CreateUser();
        EntityUserSettings settings = new() { UserID = user.ID };
        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Settings.AddSettings(settings);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        settings.AutoMergeOnRename = true;
        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Settings.UpdateSettings(settings);
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork read = _fixture.UoWFactory.Create();
        Assert.True((await read.Settings.FindSettings(user.ID, TestContext.Current.CancellationToken))?.AutoMergeOnRename);
    }

    [Fact]
    public async Task FindSettings_OfAnotherUser_ReturnsNull()
    {
        EntityUser owner = await CreateUser("owner@example.com");
        EntityUser other = await CreateUser("other@example.com");
        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Settings.AddSettings(new() { UserID = owner.ID, AutoMergeOnRename = true });
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork read = _fixture.UoWFactory.Create();

        Assert.Null(await read.Settings.FindSettings(other.ID, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AddSettings_SecondRowForOneUser_IsRejected()
    {
        EntityUser user = await CreateUser();
        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            uow.Settings.AddSettings(new() { UserID = user.ID });
            await uow.Commit(TestContext.Current.CancellationToken);
        }

        await using IUnitOfWork second = _fixture.UoWFactory.Create();
        second.Settings.AddSettings(new() { UserID = user.ID });

        await Assert.ThrowsAnyAsync<Exception>(() => second.Commit(TestContext.Current.CancellationToken));
    }
}
