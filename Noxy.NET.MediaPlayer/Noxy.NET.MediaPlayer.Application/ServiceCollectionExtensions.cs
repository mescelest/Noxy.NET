using Microsoft.Extensions.DependencyInjection.Extensions;
using Noxy.NET.MediaPlayer.Application.BackgroundServices;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Models.Upload;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Upload;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddApplication()
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

            services.AddOptions<ConfigPlayback>()
                .BindConfiguration(ConfigPlayback.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ConfigGoogle>()
                .BindConfiguration(ConfigGoogle.SectionName);

            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<IPasswordHashService, PasswordHashService>();
            services.AddSingleton<IMediaFileValidationService, MediaFileValidationService>();
            services.AddSingleton<IMediaTagWritingService, MediaTagWritingService>();
            services.AddSingleton<IUploadSignal, UploadSignal>();
            services.AddSingleton<IUploadGate, UploadGate>();
            services.AddSingleton<IUploadProcessor, UploadProcessor>();
            services.AddSingleton<IETagService, ETagService>();
            services.AddSingleton<INameKeyService, NameKeyService>();
            services.AddSingleton<ICreditSplittingService, CreditSplittingService>();
            services.AddSingleton<IMediaTagReadingService, MediaTagReadingService>();
            services.AddSingleton<IMetadataResolutionService, MetadataResolutionService>();
            services.AddSingleton<IMetadataChangeService, MetadataChangeService>();
            services.AddSingleton<INamedItemService<EntityArtist, ResponseArtist>, ArtistItemService>();
            services.AddSingleton<INamedItemService<EntityGenre, ResponseGenre>, GenreItemService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IGoogleTokenService, GoogleTokenService>();
            services.AddHostedService<CleanupService>();
            services.AddHostedService<UploadWorker>();
            services.AddHostedService<MetadataBackfillService>();
        }
    }
}
