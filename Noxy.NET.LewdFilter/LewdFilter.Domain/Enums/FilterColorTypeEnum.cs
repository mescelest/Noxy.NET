namespace LewdFilter.Domain.Enums;

public enum FilterColorTypeEnum
{
    Background,
    Text,
    Border,
}

public static class ColorTypeEnumExtensions
{
    public static string ToText(this FilterColorTypeEnum comparator) => comparator switch
    {
        FilterColorTypeEnum.Background => "Background",
        FilterColorTypeEnum.Text => "Text",
        FilterColorTypeEnum.Border => "Border",
        _ => string.Empty
    };
}
