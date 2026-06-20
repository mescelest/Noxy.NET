namespace LewdFilter.Domain.Enums;

public enum FilterIconShapeEnum
{
    Circle,
    Diamond,
    Hexagon,
    Square,
    Star,
    Triangle,
    Cross,
    Moon,
    Raindrop
}

public static class FilterIconShapeEnumExtensions
{
    extension(FilterIconShapeEnum shape)
    {
        public string ToFilterString() => shape switch
        {
            FilterIconShapeEnum.Circle => "Circle",
            FilterIconShapeEnum.Diamond => "Diamond",
            FilterIconShapeEnum.Hexagon => "Hexagon",
            FilterIconShapeEnum.Square => "Square",
            FilterIconShapeEnum.Star => "Star",
            FilterIconShapeEnum.Triangle => "Triangle",
            FilterIconShapeEnum.Cross => "Cross",
            FilterIconShapeEnum.Moon => "Moon",
            FilterIconShapeEnum.Raindrop => "Raindrop",
            _ => string.Empty
        };

        public string ToTextString() => shape switch
        {
            FilterIconShapeEnum.Circle => "● Circle",
            FilterIconShapeEnum.Diamond => "◆ Diamond",
            FilterIconShapeEnum.Hexagon => "⬢ Hexagon",
            FilterIconShapeEnum.Square => "■ Square",
            FilterIconShapeEnum.Star => "★ Star",
            FilterIconShapeEnum.Triangle => "▲ Triangle",
            FilterIconShapeEnum.Cross => "✚ Cross",
            FilterIconShapeEnum.Moon => "🌙 Moon",
            FilterIconShapeEnum.Raindrop => "💧 Raindrop",
            _ => string.Empty
        };
    }
}
