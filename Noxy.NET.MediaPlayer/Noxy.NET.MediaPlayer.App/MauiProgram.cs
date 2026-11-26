using Fluxor;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.App.Services;
using Noxy.NET.MediaPlayer.App.Stores;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
// ReSharper disable once RedundantUsingDirective
using Microsoft.AspNetCore.Components.WebView.Maui;

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
        builder.Services.AddSingleton<IPicturePicker, PicturePicker>();
        builder.Services.AddSingleton<IFolderOpener, FolderOpener>();
        builder.Services.AddNoxyNETUICommonServices();
        builder.Services.AddNoxyNETCommonServices();
#if WINDOWS
        builder.Services.AddSingleton<IGoogleSignInService, GoogleSignInService>();
        builder.Services.AddSingleton<IAudioPlayer, AudioPlayer>();
        builder.Services.AddSingleton<MediaKeyService>();
        builder.Services.AddSingleton<IMediaKeyService>(services => services.GetRequiredService<MediaKeyService>());
        builder.ConfigureLifecycleEvents(events => events.AddWindows(windows => windows.OnWindowCreated(window => IPlatformApplication.Current?.Services.GetRequiredService<MediaKeyService>().Attach(window))));

        // Windows refuses a file dragged in from outside the window (the "can't drop here" cursor) unless the WinUI web view allows drops; MAUI before 10.0.120 leaves it off.
        BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping(nameof(Microsoft.UI.Xaml.UIElement.AllowDrop), (handler, _) => handler.PlatformView.AllowDrop = true);
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

        // Created now so that it listens to the media keys from the first moment.
        _ = app.Services.GetRequiredService<IHotkeyService>();

        return app;
    }
}
