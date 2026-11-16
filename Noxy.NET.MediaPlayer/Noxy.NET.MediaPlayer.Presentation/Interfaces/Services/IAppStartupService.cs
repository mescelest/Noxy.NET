namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What the app does once at start-up: sign in silently, bring the local lists up to date and open the hub. Does what it can and never throws, so a missing address, no network or a rejected session cannot stop the app from starting.
public interface IAppStartupService
{
    Task Start();
}
