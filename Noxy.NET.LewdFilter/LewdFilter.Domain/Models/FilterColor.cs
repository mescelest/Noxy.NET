using System.Globalization;
using LewdFilter.Domain.Abstractions;

namespace LewdFilter.Domain.Models;

public record FilterColor : FilterEntity
{
    public string Name { get; set; } = string.Empty;
    public int R { get; set; } = 255;
    public int G { get; set; } = 255;
    public int B { get; set; } = 255;
    public int A { get; set; } = 255;

    public override string ToString() => A < 255 ? $"{R} {G} {B} {A}" : $"{R} {G} {B}";

    public static FilterColor DefaultBorder => new() { Name = "Default border", R = 0, G = 0, B = 0, A = 0 };
    public static FilterColor DefaultText => new() { Name = "Default text", R = 200, G = 200, B = 200, A = 255 };
    public static FilterColor DefaultBackground => new() { Name = "Default background", R = 11, G = 11, B = 11, A = 230 };

    public string ToHex()
    {
        return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }

    public bool TryUpdateFrom(string? hexInput)
    {
        if (string.IsNullOrEmpty(hexInput)) return false;

        ReadOnlySpan<char> hex = hexInput.AsSpan().TrimStart('#');
        if (hex.Length is not (6 or 8)) return false;

        try
        {
            (R, G, B, A) =
            (
                int.Parse(hex[..2], NumberStyles.HexNumber),
                int.Parse(hex[2..4], NumberStyles.HexNumber),
                int.Parse(hex[4..6], NumberStyles.HexNumber),
                hex.Length == 8 ? int.Parse(hex[6..], NumberStyles.HexNumber) : 255
            );

            return true;
        }
        catch
        {
            return false;
        }
    }
}
