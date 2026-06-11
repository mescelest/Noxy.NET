using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record LchColor(float L, float C, float H, float Alpha = 1.0f) : BaseColor
{
    public override string ToCssString() => $"lch({L}% {C} {H} / {Alpha})";

    public override RgbColor ToRgb() => ToLab().ToRgb();
    public override HexColor ToHex() => ToLab().ToHex();
    public override HslColor ToHsl() => ToLab().ToHsl();
    public override HsvColor ToHsv() => ToLab().ToHsv();
    public override XyzColor ToXyz() => ToLab().ToXyz();

    public override LabColor ToLab()
    {
        // Convert Polar (Chroma, Hue) back to Cartesian (A, B) coordinates
        float hRad = H * (MathF.PI / 180f);
        float a = C * MathF.Cos(hRad);
        float b = C * MathF.Sin(hRad);

        return new(L, a, b, Alpha);
    }

    public override LchColor ToLch() => this;
    public override OkLabColor ToOkLab() => ToXyz().ToOkLab();
    public override OkLchColor ToOkLch() => ToOkLab().ToOkLch();
}
