namespace LewdFilter.Domain.Enums;

public enum ColorTypeEnum
{
    Background,
    Text,
    Border,
}

public static class ColorTypeEnumExtensions
{
    public static string ToText(this ColorTypeEnum comparator) => comparator switch
    {
        ColorTypeEnum.Background => "Background",
        ColorTypeEnum.Text => "Text",
        ColorTypeEnum.Border => "Border",
        _ => string.Empty
    };
}
