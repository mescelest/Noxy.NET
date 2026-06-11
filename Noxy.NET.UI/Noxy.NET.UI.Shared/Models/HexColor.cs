using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.UI.Models;

public record HexColor(string HexCode) : BaseColor
{
    private string NormalizedHex => HexCode.StartsWith('#') ? HexCode : $"#{HexCode}";

    public override string ToCssString() => NormalizedHex;

    public override RgbColor ToRgb()
    {
        string cleanHex = NormalizedHex.TrimStart('#');

        cleanHex = cleanHex.Length switch
        {
            3 => $"{cleanHex[0]}{cleanHex[0]}{cleanHex[1]}{cleanHex[1]}{cleanHex[2]}{cleanHex[2]}",
            4 => $"{cleanHex[0]}{cleanHex[0]}{cleanHex[1]}{cleanHex[1]}{cleanHex[2]}{cleanHex[2]}{cleanHex[3]}{cleanHex[3]}",
            _ => cleanHex
        };

        int r = Convert.ToInt32(cleanHex[..2], 16);
        int g = Convert.ToInt32(cleanHex[2..4], 16);
        int b = Convert.ToInt32(cleanHex[4..6], 16);

        double alpha = 1.0;
        if (cleanHex.Length != 8) return new(r, g, b, alpha);

        int alphaByte = Convert.ToInt32(cleanHex[6..8], 16);
        alpha = Math.Round(alphaByte / 255.0, 2);

        return new(r, g, b, alpha);
    }

    public override HexColor ToHex() => this;
    public override HslColor ToHsl() => ToRgb().ToHsl();
    public override HsvColor ToHsv() => ToRgb().ToHsv();
    public override XyzColor ToXyz() => ToRgb().ToXyz();
    public override LabColor ToLab() => ToRgb().ToLab();
    public override LchColor ToLch() => throw new NotImplementedException();
    public override OkLabColor ToOkLab() => throw new NotImplementedException();
    public override OkLchColor ToOkLch() => ToRgb().ToOkLch();
}
