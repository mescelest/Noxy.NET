using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ShuffleModeEnumExtension
{
    extension(ShuffleModeEnum value)
    {
        // What the mode does, in words: the tooltip of the shuffle button.
        public string GetText()
        {
            return value switch
            {
                ShuffleModeEnum.Off => PresentationTexts.ShuffleOff,
                ShuffleModeEnum.Random => PresentationTexts.ShuffleRandom,
                ShuffleModeEnum.SpreadArtists => PresentationTexts.ShuffleSpreadArtists,
                _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ShuffleModeEnum))
            };
        }

        // The mode a click on the shuffle button switches to.
        public ShuffleModeEnum GetNext()
        {
            return value switch
            {
                ShuffleModeEnum.Off => ShuffleModeEnum.Random,
                ShuffleModeEnum.Random => ShuffleModeEnum.SpreadArtists,
                ShuffleModeEnum.SpreadArtists => ShuffleModeEnum.Off,
                _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ShuffleModeEnum))
            };
        }
    }
}
