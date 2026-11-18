using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FrontendAccountSwitchFlowTest
{
    private const string Password = "a-long-enough-password";

    [Fact]
    public async Task SigningInAsAnotherAccount_NeverKeepsOrShowsTheFirstAccountsLibraryFilesOrQueue()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount accountA, string emailA) = await api.CreateAccount();
        (ApiAccount accountB, string emailB) = await api.CreateAccount();
        ResponseMediaFile first = await api.UploadAndWait(accountA, "First account media");
        await api.UploadAndWait(accountB, "Second account media");
        await using FrontendClient frontend = await FrontendClient.CreateSignedIn(factory, emailA);
        await frontend.Control.StartLibrary(first.ID, LibrarySortEnum.Title, false, string.Empty);
        await frontend.Queue.SaveLocalCopy(0);
        Assert.NotNull(await frontend.FileStore.GetPath(first.ID));
        Assert.NotNull(await frontend.QueueFile.Get());

        await frontend.Session.SignOut();
        await frontend.Session.SignIn(emailB, Password);
        await frontend.LocalDataOwner.EnsureOwner();

        Assert.Empty(await frontend.Store.GetMediaFileList());
        Assert.Null(await frontend.FileStore.GetPath(first.ID));
        Assert.Empty(await frontend.FileStore.GetHeldIDList());
        Assert.True(frontend.Player.IsDisabled);
        Assert.Null(await frontend.QueueFile.Get());

        await frontend.Sync.SyncLibrary();

        Assert.Equal(["Second account media"], (await frontend.Store.GetMediaFileList()).Select(x => x.Title));
        Assert.Equal(accountB.UserID, await frontend.Store.GetOwnerUserID());
    }

    [Fact]
    public async Task SigningInAgainAsTheSameAccount_KeepsTheFilesAndTheQueue()
    {
        await using ApiFactory factory = new();
        MediaApiClient api = new(factory);
        (ApiAccount account, string email) = await api.CreateAccount();
        ResponseMediaFile media = await api.UploadAndWait(account, "My media");
        await using FrontendClient frontend = await FrontendClient.CreateSignedIn(factory, email);
        await frontend.Download.EnsureOnDevice(media.ID);
        await frontend.Control.StartLibrary(media.ID, LibrarySortEnum.Title, false, string.Empty);

        await frontend.Session.SignOut();
        await frontend.Session.SignIn(email, Password);
        await frontend.LocalDataOwner.EnsureOwner();

        Assert.NotNull(await frontend.FileStore.GetPath(media.ID));
        Assert.Single(await frontend.Store.GetMediaFileList());
        Assert.False(frontend.Player.IsDisabled);
        Assert.Equal(account.UserID, await frontend.Store.GetOwnerUserID());
    }
}
