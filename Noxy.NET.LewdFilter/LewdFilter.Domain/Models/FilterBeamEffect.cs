using LewdFilter.Domain.Enums;

namespace LewdFilter.Domain.Models;

public record FilterBeamEffect(FilterColorNameEnum Color, bool IsTemporary = false)
{
    public static FilterBeamEffect Default => new(FilterColorNameEnum.Red);

    public override string ToString()
    {
        return $"{Color}{(IsTemporary ? " Temp" : string.Empty)}";
    }
}
