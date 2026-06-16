using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record HslColor : BaseColor
{
    public int Hue { get; }
    public int Saturation { get; }
    public int Lightness { get; }
    public override double Alpha { get; }

    public HslColor(int h, int s, int l, double alpha = 1.0)
    {
        Hue = ((h % 360) + 360) % 360;
        Saturation = Math.Clamp(s, 0, 100);
        Lightness = Math.Clamp(l, 0, 100);
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"hsl({Hue} {Saturation}% {Lightness}%)" : $"hsla({Hue} {Saturation}% {Lightness}% / {AlphaCssString})";

    public override RgbColor ToRgb()
    {
        double h = Hue / 360.0;
        double s = Saturation / 100.0;
        double l = Lightness / 100.0;

        if (s == 0)
        {
            int gray = (int)Math.Round(l * 255);
            return new(gray, gray, gray, Alpha);
        }

        double q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
        double p = 2.0 * l - q;

        int r = (int)Math.Round(HueToRgb(p, q, h + 1.0 / 3.0) * 255);
        int g = (int)Math.Round(HueToRgb(p, q, h) * 255);
        int b = (int)Math.Round(HueToRgb(p, q, h - 1.0 / 3.0) * 255);

        return new(r, g, b, Alpha);

        static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1.0;
            if (t > 1) t -= 1.0;

            return t switch
            {
                < 1.0 / 6.0 => p + (q - p) * 6.0 * t,
                < 1.0 / 2.0 => q,
                < 2.0 / 3.0 => p + (q - p) * (2.0 / 3.0 - t) * 6.0,
                _ => p
            };
        }
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => this;
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();

    public static bool TryParse(string input, [NotNullWhen(true)] out HslColor? color)
    {
        color = null;
        ReadOnlySpan<char> span = input.AsSpan().Trim();
        if (!span.StartsWith("hsl", StringComparison.OrdinalIgnoreCase)) return false;
        int prefixLen = span.Length > 3 && (span[3] is 'a' or 'A') ? 5 : 4;
        if (!TryPrepareFormat(span, prefixLen, out ReadOnlySpan<char> content)) return false;

        if (!TryReadCssColorComponent(content, out ReadOnlySpan<char> tokenHue, out ReadOnlySpan<char> rem1)) return false;
        if (!TryParseAngle(tokenHue, out double deg)) return false;
        int hue = (int)Math.Round(deg);

        if (!TryReadCssColorComponent(rem1, out ReadOnlySpan<char> tokenSaturation, out ReadOnlySpan<char> rem2)) return false;
        if (tokenSaturation.EndsWith("%")) tokenSaturation = tokenSaturation[..^1];
        if (!double.TryParse(tokenSaturation, CultureInfo.InvariantCulture, out double satDouble)) return false;
        int saturation = (int)Math.Round(satDouble);

        if (!TryReadCssColorComponent(rem2, out ReadOnlySpan<char> tokenLightness, out ReadOnlySpan<char> rem3)) return false;
        if (tokenLightness.EndsWith("%")) tokenLightness = tokenLightness[..^1];
        if (!double.TryParse(tokenLightness, CultureInfo.InvariantCulture, out double lightDouble)) return false;
        int lightness = (int)Math.Round(lightDouble);

        double alpha = 1.0;
        if (!rem3.IsEmpty)
        {
            if (!TryReadCssColorComponent(rem3, out ReadOnlySpan<char> tokenAlpha, out _) || !TryParseAlpha(tokenAlpha, out alpha)) return false;
        }

        color = new(hue, saturation, lightness, alpha);
        return true;
    }
}
