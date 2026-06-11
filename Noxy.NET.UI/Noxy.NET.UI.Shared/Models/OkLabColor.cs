using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

/// <summary>
/// Represents a color in the Oklab color space (perceptually uniform).
/// </summary>
/// <param name="L">Lightness (typically 0.0 to 1.0)</param>
/// <param name="A">Green-Red axis (typically -0.4 to 0.4)</param>
/// <param name="B">Blue-Yellow axis (typically -0.4 to 0.4)</param>
/// <param name="Alpha">Transparency (0.0 to 1.0)</param>
public record OkLabColor(float L, float A, float B, float Alpha = 1.0f) : BaseColor
{
    public override string ToCssString() => $"oklab({L} {A} {B} / {Alpha})";

    public override OkLabColor ToOkLab() => this;

    public override OkLchColor ToOkLch()
    {
        float c = MathF.Sqrt(A * A + B * B);
        float h = MathF.Atan2(B, A) * (180f / MathF.PI);
        if (h < 0) h += 360f;

        return new OkLchColor(L, c, h, Alpha);
    }

    public override LchColor ToLch() => ToLab().ToLch();

    public override XyzColor ToXyz()
    {
        // 1. Oklab to LMS
        float l_ = L + 0.3963377774f * A + 0.2158037573f * B;
        float m_ = L - 0.1055613458f * A - 0.0638541728f * B;
        float s_ = L - 0.0894841775f * A - 1.2914855480f * B;

        // 2. Cube the LMS values
        float l = l_ * l_ * l_;
        float m = m_ * m_ * m_;
        float s = s_ * s_ * s_;

        // 3. LMS to XYZ (D65)
        float x = +1.2270138511f * l - 0.5577999807f * m + 0.2812561496f * s;
        float y = -0.0405801784f * l + 1.1122568696f * m - 0.0716766787f * s;
        float z = -0.0763812845f * l - 0.4214819784f * m + 1.5861632204f * s;

        return new XyzColor(x, y, z, Alpha);
    }

    public override LabColor ToLab() => ToXyz().ToLab();
    public override RgbColor ToRgb() => ToXyz().ToRgb();
    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
}
