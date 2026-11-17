using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.App;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = new(new MainPage()) { Title = "Noxy.NET.MediaPlayer.App" };

        // The queue is kept in memory while the app runs and written when it leaves the foreground: the system may end a background app without a word.
        IPlaybackReporterService? reporter = activationState?.Context.Services.GetService<IPlaybackReporterService>();
        if (reporter is not null)
        {
            window.Stopped += (_, _) => _ = Task.Run(reporter.SaveNow);
            window.Destroying += (_, _) => Task.Run(reporter.SaveNow).Wait(TimeSpan.FromSeconds(2));
        }

        return window;
    }
}
