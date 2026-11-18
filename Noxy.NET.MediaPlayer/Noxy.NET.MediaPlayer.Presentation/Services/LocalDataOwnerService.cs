using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class LocalDataOwnerService(
    ISessionService serviceSession,
    IMetadataStore storeMetadata,
    IMetadataListStore storeMetadataList,
    IListStore<ResponseSetting, string> storeSetting,
    IFileStore storeFile,
    IDownloadService serviceDownload,
    IPlayerService servicePlayer) : ILocalDataOwnerService
{
    private readonly Lock _lock = new();
    private bool _isVerified;
    private bool _subscribed;

    public event Action? Wiped;

    public event Action? Changed;

    public bool IsVerified
    {
        get
        {
            lock (_lock) return _isVerified;
        }
    }

    public async Task EnsureOwner()
    {
        SubscribeToTheSession();

        string? token = await serviceSession.GetAccessToken();
        if (token is null) return;

        // A token that cannot be read tells nothing about the owner, and a failure to read must never destroy data: nothing is wiped and the data stays shown.
        if (token.GetAccessTokenSubject() is not { } userID)
        {
            SetVerified(true);
            return;
        }

        Guid? owner = await storeMetadata.GetOwnerUserID();
        if (owner == userID)
        {
            SetVerified(true);
            return;
        }

        // A device that held data before owners were recorded keeps it: it cannot be told whose it is, and wiping it would only cost a download.
        if (owner is not null)
        {
            await Wipe();
            Wiped?.Invoke();
        }

        await storeMetadata.SetOwnerUserID(userID);
        SetVerified(true);
    }

    // The data is the previous account's again once the session ends: it must be checked once more at the next sign-in.
    private void SubscribeToTheSession()
    {
        lock (_lock)
        {
            if (_subscribed) return;

            _subscribed = true;
        }

        serviceSession.Changed += OnSessionChanged;
    }

    private void OnSessionChanged()
    {
        if (!serviceSession.IsSignedIn) SetVerified(false);
    }

    private void SetVerified(bool value)
    {
        lock (_lock)
        {
            if (_isVerified == value) return;

            _isVerified = value;
        }

        Changed?.Invoke();
    }

    // Everything of the old account goes, the player first so that no file is open: the new owner is recorded only after all of it is gone.
    private async Task Wipe()
    {
        await servicePlayer.Clear();

        foreach (Guid mediaFileID in (await storeMetadata.GetMediaFileList()).Select(x => x.ID)) await serviceDownload.Cancel(mediaFileID);

        foreach (Guid mediaFileID in await storeFile.GetPartialIDList()) await storeFile.DiscardPartial(mediaFileID);

        foreach (Guid mediaFileID in await storeFile.GetHeldIDList()) await storeFile.Delete(mediaFileID);

        // Replacing the media list also drops every file state, and replacing the playlist list every held detail.
        await storeMetadata.ReplaceMediaFileList([]);
        await storeMetadata.ReplacePlaylistList([]);
        await storeMetadata.SetLibraryETag(null);
        await storeMetadata.SetPlaylistListETag(null);

        await storeSetting.ReplaceList([]);
        await storeSetting.SetETag(null);
        await storeMetadataList.Artist.ReplaceList([]);
        await storeMetadataList.Artist.SetETag(null);
        await storeMetadataList.Album.ReplaceList([]);
        await storeMetadataList.Album.SetETag(null);
        await storeMetadataList.Genre.ReplaceList([]);
        await storeMetadataList.Genre.SetETag(null);
    }
}
