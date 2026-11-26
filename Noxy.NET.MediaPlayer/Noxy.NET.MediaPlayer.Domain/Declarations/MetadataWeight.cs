namespace Noxy.NET.MediaPlayer.Domain.Declarations;

// The weight of an artist or a genre decides where it stands in the list of a song: the higher, the earlier. Items of the same weight keep the order the song has them in.
public static class MetadataWeight
{
    public const int Minimum = 0;
    public const int Maximum = 1000;

    // What an item has until the user sets a weight: a place to move up from, and to move down from.
    public const int Default = 100;
}
