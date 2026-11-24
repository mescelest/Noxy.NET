namespace Noxy.NET.MediaPlayer.Presentation.Enums;

public enum QueueRowKindEnum
{
    // The heading above the song that is playing.
    DividerCurrent,

    // The heading above the songs the user queued.
    DividerUserQueue,

    // The heading above the songs that come from the source (a playlist or the library list).
    DividerSource,

    // A song that was played.
    History,

    // The song that is playing.
    Current,

    // A song the user queued.
    UserQueue,

    // A song that comes from the source.
    Source
}
