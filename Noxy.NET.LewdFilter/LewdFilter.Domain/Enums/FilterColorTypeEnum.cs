namespace LewdFilter.Domain.Enums;

public enum FilterColorTypeEnum
{
    Text = 0,
    Border = 1,
    Background = 2,
}

public static class ColorTypeEnumExtensions
{
    public static string ToText(this FilterColorTypeEnum comparator) => comparator switch
    {
        FilterColorTypeEnum.Text => "Text",
        FilterColorTypeEnum.Border => "Border",
        FilterColorTypeEnum.Background => "Background",
        _ => string.Empty
    };
}
