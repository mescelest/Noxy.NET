using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PresentationRegistrationTest
{
    [Fact]
    public void EveryServiceTheScreensAndTheStartupNeed_ResolvesFromTheRegistrations()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton<ISessionStorage>(new FakeSessionStorage());
        services.AddSingleton<IServerAddressStorage>(new FakeServerAddressStorage());
        services.AddSingleton<IGoogleSignInService>(new FakeGoogleSignInService());
        services.AddSingleton<IAudioPlayer>(new FakeAudioPlayer());
        services.AddSingleton<IDevicePreferenceStorage>(new FakeDevicePreferenceStorage());
        services.AddSingleton<IFileSaver>(new FakeFileSaver());
        services.AddSingleton<IFolderOpener>(new FakeFolderOpener());
        services.AddFluxor(options => options.ScanAssemblies(typeof(PresentationAssemblyMarker).Assembly));
        services.AddPresentation();
        services.AddStorage(Path.Combine(Path.GetTempPath(), $"registration-{Guid.CreateVersion7()}"));

        using ServiceProvider root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using IServiceScope scope = root.CreateScope();
        IServiceProvider provider = scope.ServiceProvider;

        Assert.NotNull(provider.GetRequiredService<IAppStartupService>());
        Assert.NotNull(provider.GetRequiredService<IPlayerService>());
        Assert.NotNull(provider.GetRequiredService<IPlaybackControlService>());
        Assert.NotNull(provider.GetRequiredService<IPlaybackReporterService>());
        Assert.NotNull(provider.GetRequiredService<IRemoteCommandHandlerService>());
        Assert.NotNull(provider.GetRequiredService<IRemotePlaybackService>());
        Assert.NotNull(provider.GetRequiredService<IQueueService>());
        Assert.NotNull(provider.GetRequiredService<IDeviceService>());
        Assert.NotNull(provider.GetRequiredService<IStateBridgeService>());
        Assert.Same(provider.GetRequiredService<IQueueService>(), provider.GetRequiredService<IQueueService>());
        Assert.NotNull(provider.GetRequiredService<IDeviceStore>());
        Assert.NotNull(provider.GetRequiredService<ILocalQueueStore>());
    }
}
