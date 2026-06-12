using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record LchColor : BaseColor
{
    public double L { get; }
    public double C { get; }
    public double H { get; }
    public double Alpha { get; }

    public LchColor(double l, double c, double h, double alpha = 1.0)
    {
        L = Math.Clamp(l, 0.0, 100.0);
        C = Math.Max(c, 0.0);
        H = ((h % 360) + 360) % 360;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"lch({L}% {C} {H})" : $"lch({L}% {C} {H} / {Alpha})";

    public override LabColor ToLab()
    {
        double hRad = H * (Math.PI / 180.0);
        double a = C * Math.Cos(hRad);
        double b = C * Math.Sin(hRad);

        return new(L, a, b, Alpha);
    }

    public override RgbColor ToRgb() => ToLab().ToRgb();
    public override HexColor ToHex() => ToLab().ToHex();
    public override HslColor ToHsl() => ToLab().ToHsl();
    public override HsvColor ToHsv() => ToLab().ToHsv();
    public override XyzColor ToXyz() => ToLab().ToXyz();
    public override LchColor ToLch() => this;
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToOkLab().ToOkLch();

    public static bool TryParse(string input, [NotNullWhen(true)] out LchColor? color)
    {
        color = null;
        ReadOnlySpan<char> span = input.AsSpan().Trim();
        if (!span.StartsWith("lch", StringComparison.OrdinalIgnoreCase) || !TryPrepareFormat(span, 3, out ReadOnlySpan<char> content)) return false;

        if (!TryReadCssColorComponent(content, out ReadOnlySpan<char> tokenLightness, out ReadOnlySpan<char> rem1)) return false;
        if (tokenLightness.EndsWith("%")) tokenLightness = tokenLightness[..^1];
        if (!double.TryParse(tokenLightness, CultureInfo.InvariantCulture, out double l)) return false;

        if (!TryReadCssColorComponent(rem1, out ReadOnlySpan<char> tokenChroma, out ReadOnlySpan<char> rem2)) return false;
        if (!double.TryParse(tokenChroma, CultureInfo.InvariantCulture, out double c)) return false;

        if (!TryReadCssColorComponent(rem2, out ReadOnlySpan<char> tokenHue, out ReadOnlySpan<char> rem3)) return false;
        if (!TryParseAngle(tokenHue, out double h)) return false;

        if (!TryReadCssColorComponent(rem3, out ReadOnlySpan<char> tokenAlpha, out _)) return false;
        if (!TryParseAlpha(tokenAlpha, out double alpha)) return false;

        color = new(l, c, h, alpha);
        return true;
    }
}
