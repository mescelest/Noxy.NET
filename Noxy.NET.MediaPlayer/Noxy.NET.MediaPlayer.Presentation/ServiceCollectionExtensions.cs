using Microsoft.Extensions.DependencyInjection.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        // The app still has to register ISessionStorage, IServerAddressStorage, IGoogleSignInService, IMetadataStore, IFileStore and IAudioPlayer.
        public IServiceCollection AddPresentation()
        {
            services.TryAddSingleton(TimeProvider.System);
            services.AddHttpClient<IAuthApiClient, AuthApiClient>(client => client.Timeout = TimeSpan.FromSeconds(PresentationConstants.HttpTimeoutSeconds));
            // The time limit is kept per request by the client itself, so an upload can take longer than a normal call.
            services.AddHttpClient<IApiClient, ApiClient>(client => client.Timeout = Timeout.InfiniteTimeSpan);
            services.AddSingleton<ISessionService, SessionService>();
            services.AddSingleton<ISyncService, SyncService>();
            services.AddSingleton<IUploadService, UploadService>();
            services.AddSingleton<ISettingService, SettingService>();
            services.AddSingleton<IMetadataSyncService, MetadataSyncService>();
            services.AddSingleton<IArtService, ArtService>();
            services.AddSingleton<IDeviceService, DeviceService>();
            services.AddSingleton<ILocalDataOwnerService, LocalDataOwnerService>();
            services.TryAddSingleton(Random.Shared);
            services.AddSingleton<IShuffleService, ShuffleService>();
            services.AddSingleton<IQueueSourceService, QueueSourceService>();
            services.AddSingleton<IDelay, Delay>();
            services.AddSingleton<IDownloadService, DownloadService>();
            services.AddSingleton<IFileReconciler, FileReconciler>();
            services.AddSingleton<IFileUseService, FileUseService>();
            services.AddSingleton<IFileTagService, FileTagService>();
            services.AddSingleton<IMediaTagReadingService, MediaTagReadingService>();
            services.AddSingleton<IMediaTagWritingService, MediaTagWritingService>();
            services.AddSingleton<IQueueService, QueueService>();
            services.AddSingleton<IPlayerService, PlayerService>();
            services.AddSingleton<IMediaHubService, MediaHubService>();
            services.AddSingleton<IRemotePlaybackService, RemotePlaybackService>();
            services.AddSingleton<IPlaybackReporterService, PlaybackReporterService>();
            services.AddSingleton<IRemoteCommandHandlerService, RemoteCommandHandlerService>();
            services.AddSingleton<IPlaybackControlService, PlaybackControlService>();
            services.AddSingleton<IHotkeyService, HotkeyService>();
            services.AddSingleton<ILibraryActionService, LibraryActionService>();
            services.AddSingleton<IQueueActionService, QueueActionService>();
            services.AddSingleton<IPlaylistService, PlaylistService>();
            services.AddSingleton<IMediaEditService, MediaEditService>();
            services.AddSingleton<IMetadataActionService, MetadataActionService>();
            services.AddSingleton<IHomeService, HomeService>();
            services.AddScoped<IStateBridgeService, StateBridgeService>();
            services.AddScoped<IDeviceResetService, DeviceResetService>();
            services.AddSingleton<IAppStartupService, AppStartupService>();

            return services;
        }
    }
}
