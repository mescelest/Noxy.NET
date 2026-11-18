using System.ComponentModel;
using Noxy.NET.UI.Enums;

namespace Noxy.NET.UI.Extensions;

public static class MaterialSymbolVariantEnumExtensions
{
    public static string ToText(this MaterialSymbolVariantEnum value)
    {
        return value switch
        {
            MaterialSymbolVariantEnum.Outlined => "material-symbols-outlined",
            MaterialSymbolVariantEnum.Filled => "material-symbols-outlined",
            MaterialSymbolVariantEnum.Rounded => "material-symbols-rounded",
            MaterialSymbolVariantEnum.RoundedFilled => "material-symbols-rounded",
            MaterialSymbolVariantEnum.Sharp => "material-symbols-sharp",
            MaterialSymbolVariantEnum.SharpFilled => "material-symbols-sharp",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(MaterialSymbolVariantEnum))
        };
    }
}
