using Fluxor;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.App.Services;
using Noxy.NET.MediaPlayer.App.Stores;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddFluxor(options =>
        {
            options.ScanAssemblies(typeof(MauiProgram).Assembly);
            options.ScanAssemblies(typeof(PresentationAssemblyMarker).Assembly);
        });
        builder.Services.AddPresentation();
        builder.Services.AddStorage(FileSystem.AppDataDirectory);
        builder.Services.AddSingleton<ISessionStorage, SecureSessionStorage>();
        builder.Services.AddSingleton<IServerAddressStorage, PreferencesServerAddressStorage>();
        builder.Services.AddSingleton<IDevicePreferenceStorage, PreferencesDevicePreferenceStorage>();
        builder.Services.AddSingleton<IFileSaver, FileSaver>();
        builder.Services.AddNoxyNETUIDialogService();
        builder.Services.AddNoxyNETUIThemeService();
#if WINDOWS
        builder.Services.AddSingleton<IGoogleSignInService, GoogleSignInService>();
        builder.Services.AddSingleton<IAudioPlayer, AudioPlayer>();
#else
        builder.Services.AddSingleton<IGoogleSignInService, UnavailableGoogleSignInService>();
#endif

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        MauiApp app = builder.Build();

        // Not awaited: it never throws, and the app must show its first screen without waiting for the network.
        _ = app.Services.GetRequiredService<IAppStartupService>().Start();

        return app;
    }
}
