using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record HsvColor : BaseColor
{
    public int Hue { get; }
    public int Saturation { get; }
    public int Value { get; }
    public override double Alpha { get; }

    public HsvColor(int hue, int saturation, int value, double alpha = 1.0)
    {
        Hue = (hue % 360 + 360) % 360;
        Saturation = Math.Clamp(saturation, 0, 100);
        Value = Math.Clamp(value, 0, 100);
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"hsv({Hue} {Saturation}% {Value}%)" : $"hsva({Hue} {Saturation}% {Value}% / {AlphaCssString})";

    public override RgbColor ToRgb()
    {
        double h = Hue / 360.0;
        double s = Saturation / 100.0;
        double v = Value / 100.0;

        if (s == 0)
        {
            int gray = (int)Math.Round(v * 255);
            return new(gray, gray, gray, Alpha);
        }

        double h6 = h * 6.0;
        int sector = (int)Math.Floor(h6);
        double fraction = h6 - sector;

        double p = v * (1.0 - s);
        double q = v * (1.0 - s * fraction);
        double t = v * (1.0 - s * (1.0 - fraction));

        (double rf, double gf, double bf) = sector switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q)
        };

        return new(
            (int)Math.Round(rf * 255),
            (int)Math.Round(gf * 255),
            (int)Math.Round(bf * 255),
            Alpha
        );
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => this;
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();

    public static bool TryParse(string input, [NotNullWhen(true)] out HsvColor? color)
    {
        color = null;
        ReadOnlySpan<char> span = input.AsSpan().Trim();
        if (!span.StartsWith("hsv", StringComparison.OrdinalIgnoreCase)) return false;

        int prefixLen = span.Length > 3 && (span[3] is 'a' or 'A') ? 5 : 4;
        if (!TryPrepareFormat(span, prefixLen, out ReadOnlySpan<char> content)) return false;

        if (!TryReadCssColorComponent(content, out ReadOnlySpan<char> tokenHue, out ReadOnlySpan<char> rem1)) return false;
        if (!TryParseAngle(tokenHue, out double deg)) return false;
        int hue = (int)Math.Round(deg);

        if (!TryReadCssColorComponent(rem1, out ReadOnlySpan<char> tokenSaturation, out ReadOnlySpan<char> rem2)) return false;
        if (tokenSaturation.EndsWith("%")) tokenSaturation = tokenSaturation[..^1];
        if (!double.TryParse(tokenSaturation, CultureInfo.InvariantCulture, out double satDouble)) return false;
        int saturation = (int)Math.Round(satDouble);

        if (!TryReadCssColorComponent(rem2, out ReadOnlySpan<char> tokenValue, out ReadOnlySpan<char> rem3)) return false;
        if (tokenValue.EndsWith("%")) tokenValue = tokenValue[..^1];
        if (!double.TryParse(tokenValue, CultureInfo.InvariantCulture, out double valDouble)) return false;
        int value = (int)Math.Round(valDouble);

        if (!TryReadAndParseAlpha(rem3, out double alpha)) return false;

        color = new(hue, saturation, value, alpha);
        return true;
    }
}
