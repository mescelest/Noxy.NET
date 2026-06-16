using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public sealed record HexColor : BaseColor
{
    public string Value { get; }
    public int Red { get; }
    public int Green { get; }
    public int Blue { get; }
    public override double Alpha { get; }

    public HexColor(int r, int g, int b, double a = 1.0)
    {
        Red = Math.Clamp(r, 0, 255);
        Green = Math.Clamp(g, 0, 255);
        Blue = Math.Clamp(b, 0, 255);
        Alpha = Math.Clamp(a, 0.0, 1.0);
        Value = Alpha >= 1.0 ? $"#{Red:X2}{Green:X2}{Blue:X2}" : $"#{Red:X2}{Green:X2}{Blue:X2}{Math.Round(Alpha * 255):X2}";
    }

    public override string ToCssString() => Value;

    public override RgbColor ToRgb() => new(Red, Green, Blue, Alpha);
    public override HexColor ToHex() => this;
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();

    public static HexColor Parse(string hex)
    {
        return !TryParse(hex, out HexColor? color) ? throw new FormatException($"Invalid hex color: {hex}") : color;
    }

    public static bool TryParse(string hex, [NotNullWhen(true)] out HexColor? color)
    {
        color = null;
        ReadOnlySpan<char> span = hex.AsSpan().Trim().TrimStart('#');
        int len = span.Length;

        if (len is not (3 or 4 or 6 or 8) || !uint.TryParse(span, NumberStyles.HexNumber, null, out uint rawHex)) return false;

        uint r, g, b;
        double a = 1.0;

        if (len is 3 or 4)
        {
            r = ((rawHex >> (len == 4 ? 12 : 8)) & 0xF) * 17;
            g = ((rawHex >> (len == 4 ? 8 : 4)) & 0xF) * 17;
            b = ((rawHex >> (len == 4 ? 4 : 0)) & 0xF) * 17;
            if (len == 4) a = ((rawHex & 0xF) * 17) / 255.0;
        }
        else
        {
            r = (rawHex >> (len == 8 ? 24 : 16)) & 0xFF;
            g = (rawHex >> (len == 8 ? 16 : 8)) & 0xFF;
            b = (rawHex >> (len == 8 ? 8 : 0)) & 0xFF;
            if (len == 8) a = (rawHex & 0xFF) / 255.0;
        }

        color = new((int)r, (int)g, (int)b, Math.Round(a, 2));
        return true;
    }
}
