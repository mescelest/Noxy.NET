using System.ComponentModel;
using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class RowSelectEnumExtension
{
    extension(RowSelectEnum mode)
    {
        // Whether a click on a row, away from its tick box, ticks or unticks it: never, only with ctrl held, or always.
        public bool IsToggledBy(bool isCtrlPressed)
        {
            return mode switch
            {
                RowSelectEnum.CheckboxOnly => false,
                RowSelectEnum.Ctrl => isCtrlPressed,
                RowSelectEnum.Click => true,
                _ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(RowSelectEnum))
            };
        }

        // The word the stylesheets know the mode by.
        public string ToText()
        {
            return mode switch
            {
                RowSelectEnum.CheckboxOnly => "checkbox",
                RowSelectEnum.Ctrl => "ctrl",
                RowSelectEnum.Click => "click",
                _ => throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(RowSelectEnum))
            };
        }
    }
}
