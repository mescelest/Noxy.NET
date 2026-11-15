using Microsoft.Extensions.DependencyInjection.Extensions;
using Noxy.NET.MediaPlayer.Application.BackgroundServices;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Gates;
using Noxy.NET.MediaPlayer.Application.Interfaces.Gates;
using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Application.Interfaces.Processors;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Interfaces.Signals;
using Noxy.NET.MediaPlayer.Application.Interfaces.Validators;
using Noxy.NET.MediaPlayer.Application.Notifiers;
using Noxy.NET.MediaPlayer.Application.Processors;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Application.Signals;
using Noxy.NET.MediaPlayer.Application.Validators;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddOptions<ConfigJwt>()
                .BindConfiguration(ConfigJwt.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ConfigCleanup>()
                .BindConfiguration(ConfigCleanup.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ConfigStorage>()
                .BindConfiguration(ConfigStorage.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ConfigUpload>()
                .BindConfiguration(ConfigUpload.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ConfigPlaylist>()
                .BindConfiguration(ConfigPlaylist.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ConfigGoogle>()
                .BindConfiguration(ConfigGoogle.SectionName);

            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<IPasswordHashService, PasswordHashService>();
            services.AddSingleton<IMediaFileValidator, MediaFileValidator>();
            services.AddSingleton<IMediaTagWriter, TagLibMediaTagWriter>();
            services.AddSingleton<IUploadSignal, UploadSignal>();
            services.AddSingleton<IUploadGate, UploadGate>();
            services.AddSingleton<IUploadProcessor, UploadProcessor>();
            services.TryAddSingleton<IUploadNotifier, NoOpUploadNotifier>();
            services.TryAddSingleton<IMediaNotifier, NoOpMediaNotifier>();
            services.TryAddSingleton<IPlaylistNotifier, NoOpPlaylistNotifier>();
            services.AddSingleton<IETagService, ETagService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IGoogleTokenService, GoogleTokenService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddHostedService<CleanupService>();
            services.AddHostedService<UploadWorker>();

            return services;
        }
    }
}
