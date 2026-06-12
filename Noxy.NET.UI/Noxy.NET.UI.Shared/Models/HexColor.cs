using System.Text.RegularExpressions;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record HexColor : BaseColor
{
    private static readonly Regex HexRegex = new(@"^#?([0-9A-Fa-f]{3,4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$", RegexOptions.Compiled);

    public string Value { get; }
    public int R { get; }
    public int G { get; }
    public int B { get; }
    public double A { get; }

    public HexColor(string hex)
    {
        if (!HexRegex.IsMatch(hex)) throw new FormatException($"Invalid hex color: {hex}");

        Value = hex;
        (R, G, B, A) = ParseHex(hex);
    }

    public override string ToCssString() => A >= 1.0 ? $"#{R:X2}{G:X2}{B:X2}" : $"#{R:X2}{G:X2}{B:X2}{Math.Round(A * 255):X2}";

    public override RgbColor ToRgb() => new(R, G, B, A);
    public override HexColor ToHex() => this;
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();

    private static (int R, int G, int B, double A) ParseHex(string hex)
    {
        string clean = hex.TrimStart('#');

        clean = clean.Length switch
        {
            3 => $"{clean[0]}{clean[0]}{clean[1]}{clean[1]}{clean[2]}{clean[2]}",
            4 => $"{clean[0]}{clean[0]}{clean[1]}{clean[1]}{clean[2]}{clean[2]}{clean[3]}{clean[3]}",
            _ => clean
        };

        int r = Convert.ToInt32(clean[..2], 16);
        int g = Convert.ToInt32(clean[2..4], 16);
        int b = Convert.ToInt32(clean[4..6], 16);

        double a = 1.0;
        if (clean.Length == 8)
        {
            int alphaByte = Convert.ToInt32(clean[6..8], 16);
            a = alphaByte / 255.0;
        }

        return (r, g, b, a);
    }
}
