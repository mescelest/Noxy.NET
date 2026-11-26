using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Declarations;

// Every key binding of the app, in one place: the handler reads it to find what a key does, and a hotkeys guide can list it.
public static class HotkeyCatalog
{
    public static readonly Hotkey Space = new("Space", HotkeyActionEnum.PlayPause, HotkeyScopeEnum.Window);
    public static readonly Hotkey MediaPlayPause = new("MediaPlayPause", HotkeyActionEnum.PlayPause, HotkeyScopeEnum.System);
    public static readonly Hotkey MediaNext = new("MediaTrackNext", HotkeyActionEnum.Next, HotkeyScopeEnum.System);
    public static readonly Hotkey MediaPrevious = new("MediaTrackPrevious", HotkeyActionEnum.Previous, HotkeyScopeEnum.System);
    public static readonly Hotkey MediaStop = new("MediaStop", HotkeyActionEnum.Stop, HotkeyScopeEnum.System);

    public static readonly IReadOnlyList<Hotkey> HotkeyList = [Space, MediaPlayPause, MediaNext, MediaPrevious, MediaStop];

    // The key is exact, as KeyboardEvent.code writes it.
    public static Hotkey? Find(string key)
    {
        return HotkeyList.FirstOrDefault(x => x.Key == key);
    }

    public static IReadOnlyList<string> KeyList(HotkeyScopeEnum scope)
    {
        return HotkeyList.Where(x => x.Scope == scope).Select(x => x.Key).ToList();
    }
}
