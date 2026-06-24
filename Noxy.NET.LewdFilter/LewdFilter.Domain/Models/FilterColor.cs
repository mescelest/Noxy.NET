using LewdFilter.Domain.Abstractions;

namespace LewdFilter.Domain.Models;

public record FilterColor : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public int Red { get; set; } = 255;
    public int Green { get; set; } = 255;
    public int Blue { get; set; } = 255;
    public int Alpha { get; set; } = 255;

    public override string ToString() => Alpha < 255 ? $"{Red} {Green} {Blue} {Alpha}" : $"{Red} {Green} {Blue}";

    public static FilterColor Default => new() { Name = "Default color", Red = 0, Green = 0, Blue = 0, Alpha = 0 };
    public static FilterColor DefaultBorder => new() { Name = "Default border", Red = 0, Green = 0, Blue = 0, Alpha = 0 };
    public static FilterColor DefaultText => new() { Name = "Default text", Red = 200, Green = 200, Blue = 200, Alpha = 255 };
    public static FilterColor DefaultBackground => new() { Name = "Default background", Red = 11, Green = 11, Blue = 11, Alpha = 230 };

    public string ToHex()
    {
        return $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";
    }

    public bool ValueEquals(FilterColor? other)
    {
        return other is not null && (ReferenceEquals(this, other) || Red == other.Red && Green == other.Green && Blue == other.Blue && Alpha == other.Alpha);
    }
}
