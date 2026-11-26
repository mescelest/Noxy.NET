using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Presentation.Declarations;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class HotkeyService : IHotkeyService
{
    private readonly IPlaybackControlService _serviceControl;
    private readonly ILogger<HotkeyService> _logger;

    // The media keys are registered only on a platform that has them.
    public HotkeyService(IPlaybackControlService serviceControl, IEnumerable<IMediaKeyService> serviceMediaKeyList, ILogger<HotkeyService> logger)
    {
        _serviceControl = serviceControl;
        _logger = logger;

        foreach (IMediaKeyService media in serviceMediaKeyList)
        {
            media.PlayPausePressed += () => _ = Raise(HotkeyCatalog.MediaPlayPause.Key);
            media.NextPressed += () => _ = Raise(HotkeyCatalog.MediaNext.Key);
            media.PreviousPressed += () => _ = Raise(HotkeyCatalog.MediaPrevious.Key);
            media.StopPressed += () => _ = Raise(HotkeyCatalog.MediaStop.Key);
        }
    }

    public async Task<bool> Press(string key)
    {
        Hotkey? hotkey = HotkeyCatalog.Find(key);
        if (hotkey is null) return false;

        Task action = hotkey.Action switch
        {
            HotkeyActionEnum.PlayPause => _serviceControl.TogglePlay(),
            HotkeyActionEnum.Next => _serviceControl.Next(),
            HotkeyActionEnum.Previous => _serviceControl.Previous(),
            HotkeyActionEnum.Stop => Stop(),
            _ => Task.CompletedTask
        };
        await action;

        return true;
    }

    private async Task Stop()
    {
        await _serviceControl.Pause();
        await _serviceControl.Seek(TimeSpan.Zero);
    }

    // A key press from the platform has nobody to throw to.
    private async Task Raise(string key)
    {
        try
        {
            await Press(key);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "The media key {Key} could not be handled.", key);
        }
    }
}
