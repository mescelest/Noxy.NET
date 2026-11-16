using Microsoft.Extensions.DependencyInjection.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        // The app still has to register ISessionStorage, IServerAddressStorage, IGoogleSignInService and IMetadataStore.
        public IServiceCollection AddPresentation()
        {
            services.TryAddSingleton(TimeProvider.System);
            services.AddHttpClient<IAuthApiClient, AuthApiClient>();
            services.AddHttpClient<IApiClient, ApiClient>();
            services.AddSingleton<ISessionService, SessionService>();
            services.AddSingleton<ISyncService, SyncService>();
            services.AddSingleton<IMediaHubService, MediaHubService>();
            services.AddSingleton<IAppStartupService, AppStartupService>();

            return services;
        }
    }
}
