using System.ComponentModel;

namespace Noxy.NET.EntityManagement.Presentation.Enums;

public enum MaterialIconVariantEnum
{
    Filled,
    Outlined,
    Round,
    Sharp,
    TwoTone
}

public static class MaterialIconVariantEnumExtensions
{
    public static string ToText(this MaterialIconVariantEnum value)
    {
        return value switch
        {
            MaterialIconVariantEnum.Filled => "material-icons",
            MaterialIconVariantEnum.Outlined => "material-icons-outlined",
            MaterialIconVariantEnum.Round => "material-icons-round",
            MaterialIconVariantEnum.Sharp => "material-icons-sharp",
            MaterialIconVariantEnum.TwoTone => "material-icons-two-tone",
            _ => throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(MaterialIconVariantEnum))
        };
    }
}
