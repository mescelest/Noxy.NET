using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record LabColor(double L, double A, double B, float Alpha = 1.0f) : BaseColor
{
    private const double Epsilon = 216.0 / 24389.0;
    private const double Kappa = 24389.0 / 27.0;

    public override string ToCssString() => $"lab({L}% {A} {B})";

    public override RgbColor ToRgb() => ToXyz().ToRgb();
    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override LabColor ToLab() => this;

    public override XyzColor ToXyz()
    {
        double fy = (L + 16.0) / 116.0;
        double fx = A / 500.0 + fy;
        double fz = fy - B / 200.0;

        double xr = Math.Pow(fx, 3) > Epsilon ? Math.Pow(fx, 3) : (116.0 * fx - 16.0) / Kappa;
        double yr = L > Kappa * Epsilon ? Math.Pow((L + 16.0) / 116.0, 3) : L / Kappa;
        double zr = Math.Pow(fz, 3) > Epsilon ? Math.Pow(fz, 3) : (116.0 * fz - 16.0) / Kappa;

        return new(xr * 0.95047, yr * 1.00000, zr * 1.08883);
    }

    public override LchColor ToLch() => throw new NotImplementedException();
    public override OkLabColor ToOkLab() => throw new NotImplementedException();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();
}
