using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record RgbColor : BaseColor
{
    public int R { get; }
    public int G { get; }
    public int B { get; }
    public double Alpha { get; }

    public RgbColor(int r, int g, int b, double alpha = 1.0)
    {
        R = Math.Clamp(r, 0, 255);
        G = Math.Clamp(g, 0, 255);
        B = Math.Clamp(b, 0, 255);
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString()
    {
        return Alpha >= 1.0 ? $"rgb({R} {G} {B})" : $"rgb({R} {G} {B} / {Alpha})";
    }

    public override RgbColor ToRgb() => this;

    public override HexColor ToHex()
    {
        int a = (int)Math.Round(Alpha * 255);
        return a == 255
            ? new($"#{R:X2}{G:X2}{B:X2}")
            : new($"#{R:X2}{G:X2}{B:X2}{a:X2}");
    }

    public override HslColor ToHsl()
    {
        double r = R / 255.0;
        double g = G / 255.0;
        double b = B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double h = 0;
        double l = (max + min) / 2.0;
        double d = max - min;
        double s = d == 0 ? 0 : l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

        if (d != 0)
        {
            if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / d + 2;
            else h = (r - g) / d + 4;

            h /= 6.0;
        }

        return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(l * 100), Alpha);
    }

    public override HsvColor ToHsv()
    {
        double r = R / 255.0;
        double g = G / 255.0;
        double b = B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double d = max - min;
        double h = 0;
        double s = max == 0 ? 0 : d / max;

        if (d != 0)
        {
            if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / d + 2;
            else h = (r - g) / d + 4;

            h /= 6.0;
        }

        return new((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(max * 100), Alpha);
    }

    public override XyzColor ToXyz()
    {
        double r = InverseGamma(R);
        double g = InverseGamma(G);
        double b = InverseGamma(B);

        double x = r * 0.4124564 + g * 0.3575761 + b * 0.1804375;
        double y = r * 0.2126729 + g * 0.7151522 + b * 0.0721750;
        double z = r * 0.0193339 + g * 0.1191920 + b * 0.9503041;

        return new XyzColor(x, y, z, Alpha);
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

        return new LabColor(l, aa, bb, a);
    }

    public override LchColor ToLch() => ToLab().ToLch();
    public override OkLabColor ToOkLab() => ToXyz().ToOkLab();
    public override OkLchColor ToOkLch() => ToOkLab().ToOkLch();

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

    public static bool TryParse(string input, out RgbColor? color)
    {
        color = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();
        if (!input.StartsWith("rgb", StringComparison.OrdinalIgnoreCase)) return false;

        int open = input.IndexOf('(');
        int close = input.LastIndexOf(')');
        if (open < 0 || close < 0 || close <= open + 1) return false;

        // Extract inside of parentheses
        ReadOnlySpan<char> inner = input.AsSpan(open + 1, close - open - 1);

        // Normalize commas → spaces
        Span<char> buffer = stackalloc char[inner.Length];
        for (int i = 0; i < inner.Length; i++)
            buffer[i] = inner[i] == ',' ? ' ' : inner[i];

        // Split on '/'
        int slashIndex = buffer.IndexOf('/');
        ReadOnlySpan<char> rgbPart = slashIndex >= 0
            ? buffer[..slashIndex].Trim()
            : buffer.Trim();

        ReadOnlySpan<char> alphaPart = slashIndex >= 0
            ? buffer[(slashIndex + 1)..].Trim()
            : ReadOnlySpan<char>.Empty;

        // Split RGB by spaces
        var rgb = rgbPart.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (rgb.Length != 3) return false;

        if (!int.TryParse(rgb[0], out int r)) return false;
        if (!int.TryParse(rgb[1], out int g)) return false;
        if (!int.TryParse(rgb[2], out int b)) return false;

        double a = 1.0;
        if (!alphaPart.IsEmpty && !double.TryParse(alphaPart, out a))
            return false;

        color = new RgbColor(r, g, b, a);
        return true;
    }
}
