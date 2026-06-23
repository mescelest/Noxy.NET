namespace LewdFilter.Domain.Enums;

public enum FilterColorNameEnum
{
    Red,
    Green,
    Blue,
    Brown,
    White,
    Yellow,
    Cyan,
    Grey,
    Orange,
    Pink,
    Purple
}

public static class FilterColorNameEnumExtensions
{
    extension(FilterColorNameEnum comparator)
    {
        public string ToFilterString() => comparator switch
        {
            FilterColorNameEnum.Red => "Red",
            FilterColorNameEnum.Green => "Green",
            FilterColorNameEnum.Blue => "Blue",
            FilterColorNameEnum.Brown => "Brown",
            FilterColorNameEnum.White => "White",
            FilterColorNameEnum.Yellow => "Yellow",
            FilterColorNameEnum.Cyan => "Cyan",
            FilterColorNameEnum.Grey => "Grey",
            FilterColorNameEnum.Orange => "Orange",
            FilterColorNameEnum.Pink => "Pink",
            FilterColorNameEnum.Purple => "Purple",
            _ => string.Empty
        };

        public string ToTextString() => comparator switch
        {
            FilterColorNameEnum.Red => "Red",
            FilterColorNameEnum.Green => "Green",
            FilterColorNameEnum.Blue => "Blue",
            FilterColorNameEnum.Brown => "Brown",
            FilterColorNameEnum.White => "White",
            FilterColorNameEnum.Yellow => "Yellow",
            FilterColorNameEnum.Cyan => "Cyan",
            FilterColorNameEnum.Grey => "Grey",
            FilterColorNameEnum.Orange => "Orange",
            FilterColorNameEnum.Pink => "Pink",
            FilterColorNameEnum.Purple => "Purple",
            _ => string.Empty
        };
    }
}
