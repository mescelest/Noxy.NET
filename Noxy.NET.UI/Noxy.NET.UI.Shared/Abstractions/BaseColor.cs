using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Abstractions;

public abstract record BaseColor
{
    public abstract double Alpha { get; }
    public string AlphaCssString => Alpha.ToString("0.00", CultureInfo.InvariantCulture);

    protected const float Tolerance = 0.00001f;

    public abstract string ToCssString();

    public abstract RgbColor ToRgb();
    public abstract HexColor ToHex();
    public abstract HslColor ToHsl();
    public abstract HsvColor ToHsv();
    public abstract XyzColor ToXyz();
    public abstract LabColor ToLab();
    public abstract LchColor ToLch();
    public abstract OkLabColor ToOkLab();
    public abstract OkLchColor ToOkLch();

    public static bool TryParseCssColor(string? value, [NotNullWhen(true)] out BaseColor? color)
    {
        color = null;
        if (string.IsNullOrWhiteSpace(value)) return false;

        ReadOnlySpan<char> span = value.AsSpan().Trim();

        if (span.StartsWith("#"))
        {
            if (!HexColor.TryParse(value, out HexColor? hexColor)) return false;
            color = hexColor;
            return true;
        }

        if (span.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            if (!RgbColor.TryParse(value, out RgbColor? rgbColor)) return false;
            color = rgbColor;
            return true;
        }

        if (span.StartsWith("hsv", StringComparison.OrdinalIgnoreCase))
        {
            if (!HsvColor.TryParse(value, out HsvColor? hsvColor)) return false;
            color = hsvColor;
            return true;
        }

        if (span.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
        {
            if (!HslColor.TryParse(value, out HslColor? hslColor)) return false;
            color = hslColor;
            return true;
        }

        if (span.StartsWith("lab", StringComparison.OrdinalIgnoreCase))
        {
            if (!LabColor.TryParse(value, out LabColor? labColor)) return false;
            color = labColor;
            return true;
        }

        if (span.StartsWith("lch", StringComparison.OrdinalIgnoreCase))
        {
            if (!LchColor.TryParse(value, out LchColor? lchColor)) return false;
            color = lchColor;
            return true;
        }

        if (span.StartsWith("oklab", StringComparison.OrdinalIgnoreCase))
        {
            if (!OkLabColor.TryParse(value, out OkLabColor? okLabColor)) return false;
            color = okLabColor;
            return true;
        }

        if (span.StartsWith("oklch", StringComparison.OrdinalIgnoreCase))
        {
            if (!OkLchColor.TryParse(value, out OkLchColor? okLchColor)) return false;
            color = okLchColor;
            return true;
        }

        return false;
    }

    protected static bool TryPrepareFormat(ReadOnlySpan<char> span, int prefixLen, out ReadOnlySpan<char> content)
    {
        content = default;
        if (span.Length <= prefixLen || span[^1] != ')') return false;

        content = span[prefixLen..^1].Trim();
        return true;
    }

    protected static bool TryReadCssColorComponent(ReadOnlySpan<char> span, out ReadOnlySpan<char> token, out ReadOnlySpan<char> remainder)
    {
        token = default;
        remainder = span;
        if (span.IsEmpty) return false;

        bool isLegacy = span.Contains(',');
        char delim1 = isLegacy ? ',' : ' ';
        char delim2 = isLegacy ? ',' : '/';

        int idx1 = span.IndexOf(delim1);
        int idx2 = span.IndexOf(delim2);

        int idx = (idx1, idx2) switch
        {
            (-1, -1) => -1,
            (-1, var j) => j,
            (var i, -1) => i,
            var (i, j) => Math.Min(i, j)
        };

        if (idx == -1)
        {
            token = span.Trim();
            remainder = ReadOnlySpan<char>.Empty;
            return !token.IsEmpty;
        }

        token = span[..idx].Trim();
        char foundDelim = span[idx];
        remainder = span[(idx + 1)..].TrimStart(foundDelim).TrimStart();
        return true;
    }

    protected static bool TryParseAlpha(ReadOnlySpan<char> token, out double alpha)
    {
        alpha = 1.0;
        if (token.IsEmpty) return true; // Missing alpha defaults to 1.0 safely

        if (!token.EndsWith("%")) return double.TryParse(token, CultureInfo.InvariantCulture, out alpha);
        if (!double.TryParse(token[..^1], CultureInfo.InvariantCulture, out double pct)) return false;
        alpha = pct / 100.0;
        return true;
    }

    protected static bool TryParsePercentageOrRaw(ReadOnlySpan<char> token, out double value, double percentageScale = 100.0)
    {
        value = 0.0;
        if (!token.EndsWith("%")) return double.TryParse(token, CultureInfo.InvariantCulture, out value);
        if (!double.TryParse(token[..^1], CultureInfo.InvariantCulture, out double pct)) return false;
        value = pct / percentageScale;
        return true;
    }

    protected static bool TryParseAngle(ReadOnlySpan<char> token, out double degrees)
    {
        degrees = 0.0;
        double factor = 1.0;
        ReadOnlySpan<char> valueToken = token;

        if (token.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
        {
            valueToken = token[..^3];
        }
        else if (token.EndsWith("rad", StringComparison.OrdinalIgnoreCase))
        {
            valueToken = token[..^3];
            factor = 180.0 / Math.PI;
        }
        else if (token.EndsWith("grad", StringComparison.OrdinalIgnoreCase))
        {
            valueToken = token[..^4];
            factor = 0.9;
        }
        else if (token.EndsWith("turn", StringComparison.OrdinalIgnoreCase))
        {
            valueToken = token[..^4];
            factor = 360.0;
        }

        if (!double.TryParse(valueToken, CultureInfo.InvariantCulture, out double rawValue)) return false;

        double deg = rawValue * factor;
        degrees = (deg % 360.0 + 360.0) % 360.0;
        return true;
    }

    protected static bool TryReadAndParseAlpha(ReadOnlySpan<char> remainder, out double alpha)
    {
        alpha = 1.0;
        return remainder.IsEmpty || TryReadCssColorComponent(remainder, out ReadOnlySpan<char> tokenAlpha, out _) && TryParseAlpha(tokenAlpha, out alpha);
    }
}
