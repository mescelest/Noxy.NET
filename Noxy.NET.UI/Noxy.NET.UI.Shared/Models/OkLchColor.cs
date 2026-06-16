using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record OkLchColor : BaseColor
{
    public double Lightness { get; }
    public double Chroma { get; }
    public double Hue { get; }
    public override double Alpha { get; }

    public OkLchColor(double lightness, double chroma, double hue, double alpha = 1.0)
    {
        Lightness = Math.Clamp(lightness, 0.0, 1.0);
        Chroma = Math.Max(chroma, 0.0);
        Hue = ((hue % 360) + 360) % 360;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"oklch({Lightness * 100}% {Chroma} {Hue})" : $"oklch({Lightness * 100}% {Chroma} {Hue} / {AlphaCssString})";

    public override RgbColor ToRgb()
    {
        double hRad = Hue * Math.PI / 180.0;
        double a = Chroma * Math.Cos(hRad);
        double b = Chroma * Math.Sin(hRad);

        double lPrime = Lightness + 0.3963377774 * a + 0.2158037573 * b;
        double mPrime = Lightness - 0.1055613458 * a - 0.0638541728 * b;
        double sPrime = Lightness - 0.0894841775 * a - 1.2914855480 * b;

        double l = lPrime * lPrime * lPrime;
        double m = mPrime * mPrime * mPrime;
        double s = sPrime * sPrime * sPrime;

        double rLinear = 4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        double gLinear = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        double bLinear = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        return new(
            ApplyGamma(rLinear),
            ApplyGamma(gLinear),
            ApplyGamma(bLinear),
            Alpha
        );
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLchColor ToOkLch() => this;

    public override OkLabColor ToOkLab()
    {
        double hRad = Hue * Math.PI / 180.0;
        double a = Chroma * Math.Cos(hRad);
        double b = Chroma * Math.Sin(hRad);
        return new(Lightness, a, b, Alpha);
    }

    public static bool TryParse(string input, [NotNullWhen(true)] out OkLchColor? color)
    {
        color = null;
        ReadOnlySpan<char> span = input.AsSpan().Trim();
        if (!span.StartsWith("oklch", StringComparison.OrdinalIgnoreCase) || !TryPrepareFormat(span, 5, out ReadOnlySpan<char> content)) return false;

        if (!TryReadCssColorComponent(content, out ReadOnlySpan<char> tokenLightness, out ReadOnlySpan<char> rem1)) return false;
        if (!TryParsePercentageOrRaw(tokenLightness, out double lightness, percentageScale: 100.0)) return false;

        if (!TryReadCssColorComponent(rem1, out ReadOnlySpan<char> tokenChroma, out ReadOnlySpan<char> rem2)) return false;
        if (!double.TryParse(tokenChroma, CultureInfo.InvariantCulture, out double chroma)) return false;

        if (!TryReadCssColorComponent(rem2, out ReadOnlySpan<char> tokenHue, out ReadOnlySpan<char> rem3)) return false;
        if (!TryParseAngle(tokenHue, out double hue)) return false;

        if (!TryReadCssColorComponent(rem3, out ReadOnlySpan<char> tokenAlpha, out _)) return false;
        if (!TryParseAlpha(tokenAlpha, out double alpha)) return false;

        color = new(lightness, chroma, hue, alpha);
        return true;
    }

    private static int ApplyGamma(double v)
    {
        v = v > 0.0031308 ? 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055 : 12.92 * v;
        return (int)Math.Clamp(Math.Round(v * 255), 0, 255);
    }
}
