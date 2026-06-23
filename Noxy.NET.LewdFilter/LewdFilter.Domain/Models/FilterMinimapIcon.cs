using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterMinimapIcon(FilterColorNameEnum Color, FilterIconSizeEnum Size, FilterIconShapeEnum Shape)
{
    public override string ToString()
    {
        return $"{Size.ToFilterString()} {Color.ToFilterString()} {Shape.ToFilterString()}";
    }
}
