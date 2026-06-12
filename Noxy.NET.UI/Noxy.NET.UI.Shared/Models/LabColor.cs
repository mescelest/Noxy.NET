using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record LabColor : BaseColor
{
    public double L { get; }
    public double A { get; }
    public double B { get; }
    public double Alpha { get; }

    private const double Epsilon = 216.0 / 24389.0;
    private const double Kappa = 24389.0 / 27.0;

    public LabColor(double l, double a, double b, double alpha = 1.0)
    {
        L = Math.Clamp(l, 0.0, 100.0);
        A = a;
        B = b;
        Alpha = Math.Clamp(alpha, 0.0, 1.0);
    }

    public override string ToCssString() => Alpha >= 1.0 ? $"lab({L}% {A} {B})" : $"lab({L}% {A} {B} / {Alpha})";

    public override RgbColor ToRgb() => ToXyz().ToRgb();
    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override LabColor ToLab() => this;

    public override XyzColor ToXyz()
    {
        double fy = (L + 16.0) / 116.0;
        double fx = fy + (A / 500.0);
        double fz = fy - (B / 200.0);

        double xr = fx * fx * fx > Epsilon ? fx * fx * fx : (116.0 * fx - 16.0) / Kappa;
        double yr = L > Kappa * Epsilon ? Math.Pow((L + 16.0) / 116.0, 3) : L / Kappa;
        double zr = fz * fz * fz > Epsilon ? fz * fz * fz : (116.0 * fz - 16.0) / Kappa;

        // D65 reference white
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
}
