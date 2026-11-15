namespace Noxy.NET.MediaPlayer.Application.Extensions;

public static class StringExtension
{
    extension(string value)
    {
        // At most maxLength characters: never half of a surrogate pair (an emoji, say), and never ending on the space the cut exposed.
        public string CutAt(int maxLength)
        {
            if (value.Length <= maxLength) return value;

            string cut = value[..maxLength];
            if (char.IsHighSurrogate(cut[^1])) cut = cut[..^1];

            return cut.TrimEnd();
        }
    }
}
