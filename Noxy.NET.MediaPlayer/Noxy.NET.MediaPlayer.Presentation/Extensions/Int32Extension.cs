namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class Int32Extension
{
    extension(int count)
    {
        // Picks the singular text for exactly one and the plural text for any other number, and puts the number in where {0} stands. Further arguments stand at {1} and on.
        public string ToCountText(string one, string many, params object[] argumentList)
        {
            return string.Format(count == 1 ? one : many, [count, .. argumentList]);
        }
    }
}
