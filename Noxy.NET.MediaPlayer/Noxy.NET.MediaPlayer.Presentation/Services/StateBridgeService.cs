using Fluxor;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation.Features.Download;
using Noxy.NET.MediaPlayer.Presentation.Features.Library;
using Noxy.NET.MediaPlayer.Presentation.Features.MetadataList;
using Noxy.NET.MediaPlayer.Presentation.Features.Player;
using Noxy.NET.MediaPlayer.Presentation.Features.PlaylistList;
using Noxy.NET.MediaPlayer.Presentation.Features.Session;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class StateBridgeService(
    IDispatcher dispatcher,
    ISyncService serviceSync,
    ISessionService serviceSession,
    IDownloadService serviceDownload,
    IFileTagService serviceFileTag,
    IPlayerService servicePlayer,
    IAppStartupService serviceStartup,
    ILocalDataOwnerService serviceLocalDataOwner,
    IMetadataStore storeMetadata,
    IMetadataListStore storeMetadataList,
    IMetadataSyncService serviceMetadataSync,
    ILogger<StateBridgeService> logger) : IStateBridgeService, IDisposable
{
    private const string ListKey = "List";
    private const string FileStateKey = "FileState";

    private readonly Lock _lock = new();

    // A read that takes a while (the library) is not started again while it runs: changes that come in meanwhile cost one more read after it.
    private readonly HashSet<string> _runningSet = [];
    private readonly HashSet<string> _pendingSet = [];
    private bool _started;
    private bool _disposed;

    public void Start()
    {
        lock (_lock)
        {
            if (_started || _disposed) return;

            _started = true;
        }

        serviceSync.Changed += OnSyncChanged;
        serviceMetadataSync.Changed += OnMetadataChanged;
        serviceDownload.Changed += OnDownloadChanged;
        serviceFileTag.Changed += OnFileTagChanged;
        servicePlayer.Changed += DispatchPlayer;
        serviceSession.Changed += DispatchSession;
        serviceStartup.Changed += DispatchSession;
        serviceLocalDataOwner.Wiped += OnWiped;
        serviceLocalDataOwner.Changed += DispatchSession;

        _ = Refresh();
    }

    // The bridge lives as long as the scope of the screens (that is where its dispatcher is): when the scope ends it stops listening, so the services do not keep it alive.
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;

            _disposed = true;
        }

        serviceSync.Changed -= OnSyncChanged;
        serviceMetadataSync.Changed -= OnMetadataChanged;
        serviceDownload.Changed -= OnDownloadChanged;
        serviceFileTag.Changed -= OnFileTagChanged;
        servicePlayer.Changed -= DispatchPlayer;
        serviceSession.Changed -= DispatchSession;
        serviceStartup.Changed -= DispatchSession;
        serviceLocalDataOwner.Wiped -= OnWiped;
        serviceLocalDataOwner.Changed -= DispatchSession;
    }

    // The old account's lists are gone from the stores: read them again, empty, without waiting for a sync that may fail.
    private void OnWiped()
    {
        _ = Refresh();
    }

    private void OnSyncChanged()
    {
        DispatchSession();
        _ = Coalesce(ListKey, LoadLists);
    }

    private void OnMetadataChanged()
    {
        _ = Coalesce(ListKey, LoadLists);
    }


    private void OnFileTagChanged()
    {
        _ = Coalesce(FileStateKey, LoadFileStates);
    }

    private void OnDownloadChanged()
    {
        DispatchDownload();
        _ = Coalesce(FileStateKey, LoadFileStates);
    }

    public async Task Refresh()
    {
        DispatchSession();
        DispatchDownload();
        DispatchPlayer();
        await Coalesce(ListKey, LoadLists);
        await Coalesce(FileStateKey, LoadFileStates);
    }

    private async Task LoadLists()
    {
        dispatcher.Dispatch(new ActionLibraryMediaLoaded(await storeMetadata.GetMediaFileList()));
        dispatcher.Dispatch(new ActionPlaylistListLoaded(await storeMetadata.GetPlaylistList()));
        dispatcher.Dispatch(new ActionMetadataListLoaded(await storeMetadataList.Artist.GetList(), await storeMetadataList.Album.GetList(), await storeMetadataList.Genre.GetList()));
    }

    private async Task LoadFileStates()
    {
        dispatcher.Dispatch(new ActionLibraryStateLoaded(await storeMetadata.GetFileStateList()));
        dispatcher.Dispatch(new ActionLibraryOutdatedLoaded(await serviceFileTag.GetOutdatedIDList()));
    }

    private void DispatchSession()
    {
        // A signed-in session is shown to the screens only once the owner of the local data was checked: before that the library, playlists and queue can still be the previous account's.
        bool hasSession = serviceSession.HasSession && (!serviceSession.IsSignedIn || serviceLocalDataOwner.IsVerified);
        dispatcher.Dispatch(new ActionSessionChanged(serviceSession.IsSignedIn, hasSession, serviceSync.IsOffline || (serviceSession.HasSession && !serviceSession.IsSignedIn), serviceStartup.IsRetrying, serviceStartup.HasRestored));
    }

    private void DispatchDownload()
    {
        dispatcher.Dispatch(new ActionDownloadChanged(serviceDownload.GetProgress()));
    }

    private void DispatchPlayer()
    {
        dispatcher.Dispatch(new ActionPlayerChanged(new(servicePlayer.Current, servicePlayer.IsDisabled, servicePlayer.IsPlaying, servicePlayer.IsWaiting, servicePlayer.Position, servicePlayer.Duration, servicePlayer.Volume,
            servicePlayer.Shuffle, servicePlayer.Repeat, servicePlayer.PlaylistID,
            servicePlayer.Queue.ItemCount)));
    }

    private async Task Coalesce(string key, Func<Task> work)
    {
        lock (_lock)
        {
            if (!_runningSet.Add(key))
            {
                _pendingSet.Add(key);
                return;
            }
        }

        while (true)
        {
            try
            {
                await work();
            }
            catch (Exception exception)
            {
                // What failed is named by its type only: the state stays as it was and the next change reads again.
                logger.LogError("Reading {Subject} for the screens failed ({ExceptionType}).", key, exception.GetType().Name);
            }

            lock (_lock)
            {
                if (!_pendingSet.Remove(key))
                {
                    _runningSet.Remove(key);
                    return;
                }
            }
        }
    }
}
