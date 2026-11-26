using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// One key binding. The key is named as the browser names it (KeyboardEvent.code); the media keys carry the same names, although they are raised by the platform.
public sealed record Hotkey(string Key, HotkeyActionEnum Action, HotkeyScopeEnum Scope);
