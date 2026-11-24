namespace Noxy.NET.MediaPlayer.Domain.Declarations;

// The hue of a playlist's tile is a place on the colour wheel in degrees.
public static class PlaylistArtHue
{
    public const int Min = 0;
    public const int Max = 359;

    // How many places there are on the wheel.
    public const int Count = Max - Min + 1;

    private const int ChoiceCount = 12;
    private const int ChoiceStep = Count / ChoiceCount;

    // The colours offered to choose from, round the wheel.
    public static IReadOnlyList<int> ChoiceList { get; } = [.. Enumerable.Range(0, ChoiceCount).Select(x => Min + x * ChoiceStep)];

    public static int PickChoice(Random random)
    {
        return ChoiceList[random.Next(ChoiceList.Count)];
    }
}
