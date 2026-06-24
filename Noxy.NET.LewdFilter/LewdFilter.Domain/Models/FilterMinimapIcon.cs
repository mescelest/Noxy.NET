using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterMinimapIcon(FilterColorNameEnum Color, FilterIconSizeEnum Size, FilterIconShapeEnum Shape)
{
    public static FilterMinimapIcon Default => new FilterMinimapIcon(FilterColorNameEnum.Red, FilterIconSizeEnum.Small, FilterIconShapeEnum.Diamond);

    public override string ToString()
    {
        return $"{Size.ToFilterString()} {Color.ToFilterString()} {Shape.ToFilterString()}";
    }
}
