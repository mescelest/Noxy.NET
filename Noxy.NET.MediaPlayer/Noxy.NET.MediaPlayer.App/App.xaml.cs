namespace Noxy.NET.MediaPlayer.App;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new(new MainPage()) { Title = "Noxy.NET.MediaPlayer.App" };
    }
}
