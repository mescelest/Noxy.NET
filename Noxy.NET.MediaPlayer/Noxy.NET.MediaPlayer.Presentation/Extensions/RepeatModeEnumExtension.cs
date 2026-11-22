using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class RepeatModeEnumExtension
{
    extension(RepeatModeEnum value)
    {
        // What the mode does, in words: the tooltip of the repeat button.
        public string GetText()
        {
            return value switch
            {
                RepeatModeEnum.No => PresentationTexts.RepeatOff,
                RepeatModeEnum.Once => PresentationTexts.RepeatOnce,
                RepeatModeEnum.Forever => PresentationTexts.RepeatForever,
                _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(RepeatModeEnum))
            };
        }

        // The mode a click on the repeat button switches to.
        public RepeatModeEnum GetNext()
        {
            return value switch
            {
                RepeatModeEnum.No => RepeatModeEnum.Once,
                RepeatModeEnum.Once => RepeatModeEnum.Forever,
                RepeatModeEnum.Forever => RepeatModeEnum.No,
                _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(RepeatModeEnum))
            };
        }
    }
}
