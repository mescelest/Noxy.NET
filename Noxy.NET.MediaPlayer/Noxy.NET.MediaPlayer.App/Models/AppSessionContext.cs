using Noxy.NET.MediaPlayer.App.Enums;

namespace Noxy.NET.MediaPlayer.App.Models;

public record AppSessionContext(AppStateEnum State, bool IsNarrow);
