using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record HsvColor(int H, int S, int V, double Alpha = 1.0) : BaseColor
{
    public override string ToCssString() => $"hsv({H}, {S}%, {V}%)";

    public override RgbColor ToRgb()
    {
        double hf = H / 360.0, sf = S / 100.0, vf = V / 100.0;
        if (S == 0)
        {
            int gray = (int)Math.Round(vf * 255);
            return new(gray, gray, gray, Alpha);
        }

        double h6 = hf * 6.0;
        if (Math.Abs(h6 - 6.0) < Tolerance)
        {
            h6 = 0.0;
        }

        int i = (int)Math.Floor(h6);
        double v1 = vf * (1.0 - sf);
        double v2 = vf * (1.0 - sf * (h6 - i));
        double v3 = vf * (1.0 - sf * (1.0 - (h6 - i)));

        (double rf, double gf, double bf) = i switch
        {
            0 => (vf, v3, v1),
            1 => (v2, vf, v1),
            2 => (v1, vf, v3),
            3 => (v1, v2, vf),
            4 => (v3, v1, vf),
            _ => (vf, v1, v2)
        };

        return new((int)Math.Round(rf * 255), (int)Math.Round(gf * 255), (int)Math.Round(bf * 255), Alpha);
    }

    public override HexColor ToHex() => ToRgb().ToHex();
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => this;
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => throw new NotImplementedException();
    public override OkLabColor ToOkLab() => throw new NotImplementedException();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();
}
