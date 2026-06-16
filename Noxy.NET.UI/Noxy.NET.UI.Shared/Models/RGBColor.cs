using System.Diagnostics.CodeAnalysis;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record RgbColor : BaseColor
{
    public int Red { get; }
    public int Green { get; }
    public int Blue { get; }
    public override double Alpha { get; }

    public RgbColor(int red, int green, int blue, double alpha = 1.0)
    {
        Red = Math.Clamp(red, 0, 255);
        Green = Math.Clamp(green, 0, 255);
        Blue = Math.Clamp(blue, 0, 255);
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString()
    {
        return Alpha >= 1.0 ? $"rgb({Red} {Green} {Blue})" : $"rgb({Red} {Green} {Blue} / {AlphaCssString})";
    }

    public override RgbColor ToRgb() => this;
    public override HexColor ToHex() => new(Red, Green, Blue, Alpha);

    public override HslColor ToHsl()
    {
        double r = Red / 255.0;
        double g = Green / 255.0;
        double b = Blue / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double h = 0;
        double l = (max + min) / 2.0;
        double d = max - min;
        double s = d == 0 ? 0 : l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

        if (d == 0) return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(l * 100), Alpha);

        if (Math.Abs(max - r) < Tolerance)
        {
            h = (g - b) / d + (g < b ? 6 : 0);
        }
        else if (Math.Abs(max - g) < Tolerance)
        {
            h = (b - r) / d + 2;
        }
        else
        {
            h = (r - g) / d + 4;
        }

        h /= 6.0;

        return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(l * 100), Alpha);
    }

    public override HsvColor ToHsv()
    {
        double r = Red / 255.0;
        double g = Green / 255.0;
        double b = Blue / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double d = max - min;
        double h = 0;
        double s = max == 0 ? 0 : d / max;

        if (d == 0) return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(max * 100), Alpha);

        if (Math.Abs(max - r) < Tolerance)
        {
            h = (g - b) / d + (g < b ? 6 : 0);
        }
        else if (Math.Abs(max - g) < Tolerance)
        {
            h = (b - r) / d + 2;
        }
        else
        {
            h = (r - g) / d + 4;
        }

        h /= 6.0;

        return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(max * 100), Alpha);
    }

    public override XyzColor ToXyz()
    {
        double r = InverseGamma(Red);
        double g = InverseGamma(Green);
        double b = InverseGamma(Blue);

        double x = r * 0.4124564 + g * 0.3575761 + b * 0.1804375;
        double y = r * 0.2126729 + g * 0.7151522 + b * 0.0721750;
        double z = r * 0.0193339 + g * 0.1191920 + b * 0.9503041;

        return new(x, y, z, Alpha);
    }

    public override LabColor ToLab()
    {
        (double x, double y, double z, double a) = ToXyz();

        double xr = x / 0.95047;
        double yr = y / 1.00000;
        double zr = z / 1.08883;

        double fx = Perceptual(xr);
        double fy = Perceptual(yr);
        double fz = Perceptual(zr);

        double l = 116.0 * fy - 16.0;
        double aa = 500.0 * (fx - fy);
        double bb = 200.0 * (fy - fz);

        return new(l, aa, bb, a);
    }

    public override LchColor ToLch() => ToLab().ToLch();
    public override OkLabColor ToOkLab() => ToXyz().ToOkLab();
    public override OkLchColor ToOkLch() => ToOkLab().ToOkLch();

    public static bool TryParse(string input, [NotNullWhen(true)] out RgbColor? color)
    {
        color = null;
        ReadOnlySpan<char> span = input.AsSpan().Trim();
        if (!span.StartsWith("rgb", StringComparison.OrdinalIgnoreCase)) return false;
        int prefixLen = span.Length > 3 && (span[3] is 'a' or 'A') ? 4 : 3;
        if (!TryPrepareFormat(span, prefixLen, out ReadOnlySpan<char> content)) return false;

        if (!TryReadCssColorComponent(content, out ReadOnlySpan<char> tokenRed, out ReadOnlySpan<char> rem1)) return false;
        if (!TryParsePercentageOrRaw(tokenRed, out double rVal, percentageScale: 100.0)) return false;
        int red = tokenRed.EndsWith("%") ? (int)Math.Round(rVal * 255.0) : (int)Math.Round(rVal);

        if (!TryReadCssColorComponent(rem1, out ReadOnlySpan<char> tokenGreen, out ReadOnlySpan<char> rem2)) return false;
        if (!TryParsePercentageOrRaw(tokenGreen, out double gVal, percentageScale: 100.0)) return false;
        int green = tokenGreen.EndsWith("%") ? (int)Math.Round(gVal * 255.0) : (int)Math.Round(gVal);

        if (!TryReadCssColorComponent(rem2, out ReadOnlySpan<char> tokenBlue, out ReadOnlySpan<char> rem3)) return false;
        if (!TryParsePercentageOrRaw(tokenBlue, out double bVal, percentageScale: 100.0)) return false;
        int blue = tokenBlue.EndsWith("%") ? (int)Math.Round(bVal * 255.0) : (int)Math.Round(bVal);

        if (!TryReadCssColorComponent(rem3, out ReadOnlySpan<char> tokenAlpha, out _)) return false;
        if (!TryParseAlpha(tokenAlpha, out double alpha)) return false;

        color = new(red, green, blue, alpha);
        return true;
    }

    private static double InverseGamma(int v)
    {
        double x = v / 255.0;
        return x > 0.04045 ? Math.Pow((x + 0.055) / 1.055, 2.4) : x / 12.92;
    }

    private static double Perceptual(double v)
    {
        const double d = 0.20689655172413793;
        const double t = d * d * d;
        return v > t ? Math.Cbrt(v) : v / (3.0 * d * d) + 4.0 / 29.0;
    }
}
