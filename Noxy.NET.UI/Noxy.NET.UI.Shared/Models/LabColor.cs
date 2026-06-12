using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record LabColor : BaseColor
{
    public double Lightness { get; }
    public double AxisA { get; }
    public double AxisB { get; }
    public double Alpha { get; }

    private const double Epsilon = 216.0 / 24389.0;
    private const double Kappa = 24389.0 / 27.0;

    public LabColor(double lightness, double axisA, double axisB, double alpha = 1.0)
    {
        Lightness = Math.Clamp(lightness, 0.0, 100.0);
        AxisA = axisA;
        AxisB = axisB;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"lab({Lightness}% {AxisA} {AxisB})" : $"lab({Lightness}% {AxisA} {AxisB} / {Alpha})";

    public override RgbColor ToRgb() => ToXyz().ToRgb();
    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override LabColor ToLab() => this;

    public override XyzColor ToXyz()
    {
        double fy = (Lightness + 16.0) / 116.0;
        double fx = fy + (AxisA / 500.0);
        double fz = fy - (AxisB / 200.0);

        double xr = fx * fx * fx > Epsilon ? fx * fx * fx : (116.0 * fx - 16.0) / Kappa;
        double yr = Lightness > Kappa * Epsilon ? Math.Pow((Lightness + 16.0) / 116.0, 3) : Lightness / Kappa;
        double zr = fz * fz * fz > Epsilon ? fz * fz * fz : (116.0 * fz - 16.0) / Kappa;

        return new(
            xr * 0.95047,
            yr * 1.00000,
            zr * 1.08883,
            Alpha
        );
    }

    public override LchColor ToLch() => ToRgb().ToLch();
    public override OkLabColor ToOkLab() => ToRgb().ToOkLab();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();

    public static bool TryParse(string input, [NotNullWhen(true)] out LabColor? color)
    {
        color = null;
        ReadOnlySpan<char> span = input.AsSpan().Trim();
        if (!span.StartsWith("lab", StringComparison.OrdinalIgnoreCase) || !TryPrepareFormat(span, 3, out ReadOnlySpan<char> content)) return false;

        if (!TryReadCssColorComponent(content, out ReadOnlySpan<char> tokenLightness, out ReadOnlySpan<char> rem1)) return false;
        if (tokenLightness.EndsWith("%")) tokenLightness = tokenLightness[..^1];
        if (!double.TryParse(tokenLightness, CultureInfo.InvariantCulture, out double lightness)) return false;

        if (!TryReadCssColorComponent(rem1, out ReadOnlySpan<char> tokenAxisA, out ReadOnlySpan<char> rem2)) return false;
        if (!double.TryParse(tokenAxisA, CultureInfo.InvariantCulture, out double axisA)) return false;

        if (!TryReadCssColorComponent(rem2, out ReadOnlySpan<char> tokenAxisB, out ReadOnlySpan<char> rem3)) return false;
        if (!double.TryParse(tokenAxisB, CultureInfo.InvariantCulture, out double axisB)) return false;

        if (!TryReadCssColorComponent(rem3, out ReadOnlySpan<char> tokenAlpha, out _)) return false;
        if (!TryParseAlpha(tokenAlpha, out double alpha)) return false;

        color = new(lightness, axisA, axisB, alpha);
        return true;
    }
}
