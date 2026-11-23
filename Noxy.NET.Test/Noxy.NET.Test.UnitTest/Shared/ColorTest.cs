using System.Globalization;
using Noxy.NET.UI.Models.Abstractions;
using Noxy.NET.UI.Models.Colors;

namespace Noxy.NET.Test.Shared;

public class ColorTest
{
    // The colours the conversions are tried on: the corners of the cube, a grey, and a few in between.
    public static TheoryData<int, int, int> SampleList => new()
    {
        { 0, 0, 0 },
        { 255, 255, 255 },
        { 255, 0, 0 },
        { 0, 255, 0 },
        { 0, 0, 255 },
        { 128, 128, 128 },
        { 200, 80, 40 },
        { 77, 74, 211 },
        { 35, 136, 61 },
        { 217, 127, 0 }
    };

    private static void AssertClose(RgbColor expected, RgbColor actual, int tolerance)
    {
        Assert.InRange(actual.Red, expected.Red - tolerance, expected.Red + tolerance);
        Assert.InRange(actual.Green, expected.Green - tolerance, expected.Green + tolerance);
        Assert.InRange(actual.Blue, expected.Blue - tolerance, expected.Blue + tolerance);
    }

    private static void WithCulture(string name, Action action)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new(name);
        try
        {
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    #region -- Hex --

    [Theory]
    [InlineData("#F00", 255, 0, 0)]
    [InlineData("#00ff00", 0, 255, 0)]
    [InlineData("0000FF", 0, 0, 255)]
    [InlineData("  #4D4AD3  ", 77, 74, 211)]
    public void Hex_ParsesShortAndLongForms_WithOrWithoutTheHash(string input, int red, int green, int blue)
    {
        Assert.True(HexColor.TryParse(input, out HexColor? color));
        Assert.Equal((red, green, blue), (color.Red, color.Green, color.Blue));
        Assert.Equal(1.0, color.Alpha);
    }

    [Theory]
    [InlineData("#FF000080", 0.5)]
    [InlineData("#F008", 0.53)]
    public void Hex_ParsesAnAlphaChannel(string input, double alpha)
    {
        Assert.True(HexColor.TryParse(input, out HexColor? color));
        Assert.Equal(alpha, color.Alpha, 2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("#")]
    [InlineData("#12")]
    [InlineData("#12345")]
    [InlineData("#GGGGGG")]
    [InlineData("rgb(1 2 3)")]
    public void Hex_RefusesWhatIsNotAColour(string input)
    {
        Assert.False(HexColor.TryParse(input, out _));
        Assert.Throws<FormatException>(() => HexColor.Parse(input));
    }

    [Fact]
    public void Hex_WritesUpperCaseDigits()
    {
        Assert.Equal("#FF8000", new RgbColor(255, 128, 0).ToHex().Value);
    }

    [Fact]
    public void Hex_WithAnAlpha_WritesTheAlphaByte_InsteadOfThrowing()
    {
        Assert.Equal("#FF800080", new RgbColor(255, 128, 0, 0.5).ToHex().Value);
        Assert.Equal("#00000000", new RgbColor(0, 0, 0, 0).ToHex().Value);
    }

    #endregion -- Hex --

    #region -- Known values --

    [Theory]
    [InlineData(255, 0, 0, 0, 100, 50)]
    [InlineData(0, 255, 0, 120, 100, 50)]
    [InlineData(0, 0, 255, 240, 100, 50)]
    [InlineData(255, 255, 255, 0, 0, 100)]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(128, 128, 128, 0, 0, 50)]
    public void Rgb_ToHsl_GivesTheKnownValues(int red, int green, int blue, int hue, int saturation, int lightness)
    {
        HslColor hsl = new RgbColor(red, green, blue).ToHsl();

        Assert.Equal((hue, saturation, lightness), (hsl.Hue, hsl.Saturation, hsl.Lightness));
    }

    [Theory]
    [InlineData(255, 0, 0, 0, 100, 100)]
    [InlineData(0, 0, 255, 240, 100, 100)]
    [InlineData(255, 255, 255, 0, 0, 100)]
    [InlineData(0, 0, 0, 0, 0, 0)]
    public void Rgb_ToHsv_GivesTheKnownValues(int red, int green, int blue, int hue, int saturation, int value)
    {
        HsvColor hsv = new RgbColor(red, green, blue).ToHsv();

        Assert.Equal((hue, saturation, value), (hsv.Hue, hsv.Saturation, hsv.Value));
    }

    [Fact]
    public void Rgb_ToLab_GivesTheKnownValuesOfRed_AndWhite()
    {
        LabColor red = new RgbColor(255, 0, 0).ToLab();
        Assert.Equal(53.24, red.Lightness, 1);
        Assert.Equal(80.09, red.AxisA, 1);
        Assert.Equal(67.20, red.AxisB, 1);

        LabColor white = new RgbColor(255, 255, 255).ToLab();
        Assert.Equal(100.0, white.Lightness, 1);
        Assert.Equal(0.0, white.AxisA, 1);
        Assert.Equal(0.0, white.AxisB, 1);
    }

    [Fact]
    public void Rgb_ToOkLab_GivesTheKnownValuesOfRed_AndWhite()
    {
        OkLabColor red = new RgbColor(255, 0, 0).ToOkLab();
        Assert.Equal(0.6280, red.Lightness, 3);
        Assert.Equal(0.2249, red.AxisA, 3);
        Assert.Equal(0.1258, red.AxisB, 3);

        OkLabColor white = new RgbColor(255, 255, 255).ToOkLab();
        Assert.Equal(1.0, white.Lightness, 3);
        Assert.Equal(0.0, white.AxisA, 3);
        Assert.Equal(0.0, white.AxisB, 3);
    }

    [Fact]
    public void Rgb_ToOkLch_GivesTheKnownValuesOfRed()
    {
        OkLchColor red = new RgbColor(255, 0, 0).ToOkLch();

        Assert.Equal(0.6280, red.Lightness, 3);
        Assert.Equal(0.2577, red.Chroma, 3);
        Assert.Equal(29.23, red.Hue, 1);
    }

    [Fact]
    public void Lab_ToLch_IsThePolarForm()
    {
        LchColor lch = new LabColor(50, 3, 4).ToLch();

        Assert.Equal(50.0, lch.Lightness);
        Assert.Equal(5.0, lch.Chroma, 6);
        Assert.Equal(53.13, lch.Hue, 2);
    }

    [Fact]
    public void Lab_ToLch_KeepsTheHueBetweenZeroAndThreeSixty()
    {
        LchColor lch = new LabColor(50, 3, -4).ToLch();

        Assert.Equal(306.87, lch.Hue, 2);
    }

    [Fact]
    public void TheConstructors_KeepTheValuesInRange()
    {
        Assert.Equal(10, new HslColor(370, 0, 0).Hue);
        Assert.Equal(350, new HslColor(-10, 0, 0).Hue);
        Assert.Equal(100, new HslColor(0, 150, 0).Saturation);
        Assert.Equal(255, new RgbColor(300, -5, 0).Red);
        Assert.Equal(0, new RgbColor(300, -5, 0).Green);
        Assert.Equal(1.0, new RgbColor(0, 0, 0, 7).Alpha);
        Assert.Equal(0.0, new RgbColor(0, 0, 0, -1).Alpha);
        Assert.Equal(1.0, new OkLchColor(3, 0.1, 0).Lightness);
        Assert.Equal(0.0, new LchColor(50, -4, 0).Chroma);
    }

    #endregion -- Known values --

    #region -- Converting between all of them --

    [Theory]
    [MemberData(nameof(SampleList))]
    public void EveryColour_CanBeConvertedToEveryOther(int red, int green, int blue)
    {
        RgbColor rgb = new(red, green, blue, 0.75);
        List<BaseColor> startList = [rgb, rgb.ToHex(), rgb.ToHsl(), rgb.ToHsv(), rgb.ToXyz(), rgb.ToLab(), rgb.ToLch(), rgb.ToOkLab(), rgb.ToOkLch()];

        // Each pair of conversions must end: a pair that calls the other one never returns, and takes the process with it.
        foreach (BaseColor start in startList)
        {
            Assert.NotNull(start.ToRgb());
            Assert.NotNull(start.ToHex());
            Assert.NotNull(start.ToHsl());
            Assert.NotNull(start.ToHsv());
            Assert.NotNull(start.ToXyz());
            Assert.NotNull(start.ToLab());
            Assert.NotNull(start.ToLch());
            Assert.NotNull(start.ToOkLab());
            Assert.NotNull(start.ToOkLch());
        }
    }

    [Theory]
    [MemberData(nameof(SampleList))]
    public void ConvertingThere_AndBack_GivesTheSameColour(int red, int green, int blue)
    {
        RgbColor rgb = new(red, green, blue);

        AssertClose(rgb, rgb.ToHex().ToRgb(), 0);
        AssertClose(rgb, rgb.ToXyz().ToRgb(), 1);
        AssertClose(rgb, rgb.ToLab().ToRgb(), 1);
        AssertClose(rgb, rgb.ToLch().ToRgb(), 1);
        AssertClose(rgb, rgb.ToOkLab().ToRgb(), 1);
        AssertClose(rgb, rgb.ToOkLch().ToRgb(), 1);

        // Hue, saturation and lightness are whole numbers, so they lose a little.
        AssertClose(rgb, rgb.ToHsl().ToRgb(), 3);
        AssertClose(rgb, rgb.ToHsv().ToRgb(), 3);
    }

    [Theory]
    [MemberData(nameof(SampleList))]
    public void TheShortWayAndTheLongWay_AgreeWithEachOther(int red, int green, int blue)
    {
        RgbColor rgb = new(red, green, blue);

        // Lab to Lch directly, and the other end of the cube through RGB.
        LchColor direct = rgb.ToLab().ToLch();
        LchColor viaOkLab = rgb.ToOkLab().ToLch();

        Assert.Equal(direct.Lightness, viaOkLab.Lightness, 0);
        Assert.Equal(rgb.ToOkLch().Hue, rgb.ToOkLab().ToOkLch().Hue, 6);
    }

    [Fact]
    public void Conversions_KeepTheAlpha()
    {
        RgbColor rgb = new(200, 80, 40, 0.25);

        Assert.Equal(0.25, rgb.ToHsl().Alpha);
        Assert.Equal(0.25, rgb.ToHsv().Alpha);
        Assert.Equal(0.25, rgb.ToXyz().Alpha);
        Assert.Equal(0.25, rgb.ToLab().Alpha);
        Assert.Equal(0.25, rgb.ToLch().Alpha);
        Assert.Equal(0.25, rgb.ToOkLab().Alpha);
        Assert.Equal(0.25, rgb.ToOkLch().Alpha);
        Assert.Equal(0.25, rgb.ToOkLch().ToRgb().Alpha);
    }

    #endregion -- Converting between all of them --

    #region -- CSS strings --

    [Fact]
    public void ToCssString_WritesTheStandardForms()
    {
        Assert.Equal("rgb(10 20 30)", new RgbColor(10, 20, 30).ToCssString());
        Assert.Equal("rgb(10 20 30 / 0.50)", new RgbColor(10, 20, 30, 0.5).ToCssString());
        Assert.Equal("hsl(10 50% 30%)", new HslColor(10, 50, 30).ToCssString());
        Assert.Equal("hsla(10 50% 30% / 0.25)", new HslColor(10, 50, 30, 0.25).ToCssString());
        Assert.Equal("#0A141E", new HexColor(10, 20, 30).ToCssString());
        Assert.Equal("lab(50.5% 10.5 -20.5)", new LabColor(50.5, 10.5, -20.5).ToCssString());
        Assert.Equal("lch(50.5% 10.5 20.5)", new LchColor(50.5, 10.5, 20.5).ToCssString());
        Assert.Equal("oklab(0.655 0.1 -0.1)", new OkLabColor(0.655, 0.1, -0.1).ToCssString());
        Assert.Equal("oklch(65.5% 0.21 250.5)", new OkLchColor(0.655, 0.21, 250.5).ToCssString());
    }

    [Fact]
    public void ToCssString_LeavesOutTheNoiseOfFloatingPoint()
    {
        // 0.655 * 100 is 65.50000000000001 in floating point.
        Assert.Equal("oklch(65.5% 0.21 250.5)", new OkLchColor(0.655, 0.21, 250.5).ToCssString());
        Assert.Equal("lab(33.3333% 0.1 0.2)", new LabColor(100.0 / 3, 0.1, 0.2).ToCssString());
    }

    [Theory]
    [InlineData("da-DK")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    public void ToCssString_WritesAPoint_InACultureThatWritesAComma(string culture)
    {
        List<BaseColor> colorList =
        [
            new RgbColor(10, 20, 30, 0.5),
            new HexColor(10, 20, 30, 0.5),
            new HslColor(10, 50, 30, 0.5),
            new HsvColor(10, 50, 30, 0.5),
            new LabColor(50.5, 10.5, -20.5, 0.5),
            new LchColor(50.5, 10.5, 20.5, 0.5),
            new OkLabColor(0.655, 0.1, -0.1, 0.5),
            new OkLchColor(0.655, 0.21, 250.5, 0.5)
        ];
        List<string> invariantList = [];
        WithCulture("en-US", () => invariantList.AddRange(colorList.Select(x => x.ToCssString())));

        WithCulture(culture, () =>
        {
            List<string> actualList = colorList.Select(x => x.ToCssString()).ToList();

            Assert.Equal(invariantList, actualList);
            Assert.All(actualList, x => Assert.DoesNotContain(',', x));
        });
    }

    [Theory]
    [InlineData("da-DK")]
    [InlineData("en-US")]
    public void ToCssString_CanBeReadBackInAnyCulture(string culture)
    {
        WithCulture(culture, () =>
        {
            BaseColor original = new OkLchColor(0.655, 0.21, 250.5, 0.5);

            Assert.True(BaseColor.TryParseCssColor(original.ToCssString(), out BaseColor? parsed));
            AssertClose(original.ToRgb(), parsed.ToRgb(), 1);
        });
    }

    [Fact]
    public void ToCssString_OfXyz_IsNotSupported_ForCssHasNoSuchColour()
    {
        Assert.Throws<NotSupportedException>(() => new XyzColor(0.1, 0.2, 0.3).ToCssString());
    }

    public static TheoryData<BaseColor> RoundTripList => new()
    {
        new RgbColor(10, 20, 30),
        new RgbColor(10, 20, 30, 0.5),
        new HexColor(10, 20, 30),
        new HexColor(10, 20, 30, 0.5),
        new HslColor(10, 50, 30),
        new HslColor(10, 50, 30, 0.25),
        new HsvColor(200, 40, 80),
        new HsvColor(200, 40, 80, 0.25),
        new LabColor(50.5, 10.5, -20.5),
        new LabColor(50.5, 10.5, -20.5, 0.5),
        new LchColor(50.5, 10.5, 20.5),
        new LchColor(50.5, 10.5, 20.5, 0.5),
        new OkLabColor(0.655, 0.1, -0.1),
        new OkLabColor(0.655, 0.1, -0.1, 0.5),
        new OkLchColor(0.655, 0.21, 250.5),
        new OkLchColor(0.655, 0.21, 250.5, 0.5)
    };

    [Theory]
    [MemberData(nameof(RoundTripList))]
    public void ToCssString_CanBeReadBack_WithItsAlpha(BaseColor original)
    {
        Assert.True(BaseColor.TryParseCssColor(original.ToCssString(), out BaseColor? parsed));

        Assert.Equal(original.GetType(), parsed.GetType());
        Assert.Equal(original.Alpha, parsed.Alpha, 2);
        AssertClose(original.ToRgb(), parsed.ToRgb(), 1);
    }

    #endregion -- CSS strings --

    #region -- Parsing --

    [Theory]
    [InlineData("rgb(255, 0, 0)", 255, 0, 0, 1.0)]
    [InlineData("rgb(255 0 0)", 255, 0, 0, 1.0)]
    [InlineData("RGB(255 0 0)", 255, 0, 0, 1.0)]
    [InlineData("rgba(255, 0, 0, 0.5)", 255, 0, 0, 0.5)]
    [InlineData("rgba(255,0,0,0.5)", 255, 0, 0, 0.5)]
    [InlineData("rgb(255 0 0 / 0.5)", 255, 0, 0, 0.5)]
    [InlineData("rgb(255 0 0 / 50%)", 255, 0, 0, 0.5)]
    [InlineData("rgb(255 0 0/50%)", 255, 0, 0, 0.5)]
    [InlineData("rgb(100% 0% 50%)", 255, 0, 128, 1.0)]
    [InlineData("  rgb(1 2 3)  ", 1, 2, 3, 1.0)]
    public void Rgb_ParsesTheOldAndTheNewSyntax(string input, int red, int green, int blue, double alpha)
    {
        Assert.True(RgbColor.TryParse(input, out RgbColor? color));

        Assert.Equal((red, green, blue), (color.Red, color.Green, color.Blue));
        Assert.Equal(alpha, color.Alpha, 2);
    }

    [Theory]
    [InlineData("hsl(120 100% 50%)", 120, 100, 50)]
    [InlineData("hsl(120, 100%, 50%)", 120, 100, 50)]
    [InlineData("hsl(120deg 100% 50%)", 120, 100, 50)]
    [InlineData("hsl(0.5turn 100% 50%)", 180, 100, 50)]
    [InlineData("hsl(200grad 100% 50%)", 180, 100, 50)]
    [InlineData("hsl(-90 100% 50%)", 270, 100, 50)]
    [InlineData("hsla(120 100% 50% / 0.5)", 120, 100, 50)]
    public void Hsl_ParsesTheUnitsOfAnAngle(string input, int hue, int saturation, int lightness)
    {
        Assert.True(HslColor.TryParse(input, out HslColor? color));

        Assert.Equal((hue, saturation, lightness), (color.Hue, color.Saturation, color.Lightness));
    }

    [Theory]
    [InlineData("lab(50% 40 59.5)", 50.0, 40.0, 59.5)]
    [InlineData("lab(50 40 59.5)", 50.0, 40.0, 59.5)]
    public void Lab_Parses(string input, double lightness, double axisA, double axisB)
    {
        Assert.True(LabColor.TryParse(input, out LabColor? color));

        Assert.Equal((lightness, axisA, axisB), (color.Lightness, color.AxisA, color.AxisB));
    }

    [Theory]
    [InlineData("oklab(0.5 0.1 -0.1)", 0.5, 0.1, -0.1)]
    [InlineData("oklab(50% 0.1 -0.1)", 0.5, 0.1, -0.1)]
    [InlineData("OKLAB(0.5 0.1 -0.1 / 0.5)", 0.5, 0.1, -0.1)]
    public void OkLab_Parses(string input, double lightness, double axisA, double axisB)
    {
        Assert.True(OkLabColor.TryParse(input, out OkLabColor? color));

        Assert.Equal((lightness, axisA, axisB), (color.Lightness, color.AxisA, color.AxisB));
    }

    [Theory]
    [InlineData("oklch(0.65 0.2 250)", 0.65, 0.2, 250.0)]
    [InlineData("oklch(65% 0.2 250)", 0.65, 0.2, 250.0)]
    [InlineData("oklch(65% 0.2 250deg)", 0.65, 0.2, 250.0)]
    [InlineData("oklch(65% 0.2 0.5turn / 0.5)", 0.65, 0.2, 180.0)]
    public void OkLch_Parses(string input, double lightness, double chroma, double hue)
    {
        Assert.True(OkLchColor.TryParse(input, out OkLchColor? color));

        Assert.Equal(lightness, color.Lightness, 6);
        Assert.Equal(chroma, color.Chroma, 6);
        Assert.Equal(hue, color.Hue, 6);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("banana")]
    [InlineData("rgb(")]
    [InlineData("rgb()")]
    [InlineData("rgb(1 2)")]
    [InlineData("rgb(a b c)")]
    [InlineData("rgb(1 2 3")]
    [InlineData("hsl(1 2)")]
    [InlineData("lab(1 2)")]
    [InlineData("oklch(1 2)")]
    [InlineData("oklab(")]
    [InlineData("#12")]
    public void ParseCssColor_RefusesWhatIsNotAColour(string input)
    {
        Assert.False(BaseColor.TryParseCssColor(input, out BaseColor? color));
        Assert.Null(color);
    }

    [Fact]
    public void ParseCssColor_RefusesNull()
    {
        Assert.False(BaseColor.TryParseCssColor(null, out _));
    }

    [Theory]
    [InlineData("#F00", typeof(HexColor))]
    [InlineData("rgb(1 2 3)", typeof(RgbColor))]
    [InlineData("hsl(1 2% 3%)", typeof(HslColor))]
    [InlineData("hsv(1 2% 3%)", typeof(HsvColor))]
    [InlineData("lab(1% 2 3)", typeof(LabColor))]
    [InlineData("lch(1% 2 3)", typeof(LchColor))]
    [InlineData("oklab(0.1 0.2 0.3)", typeof(OkLabColor))]
    [InlineData("oklch(1% 0.2 3)", typeof(OkLchColor))]
    public void ParseCssColor_PicksTheTypeFromTheName(string input, Type expected)
    {
        Assert.True(BaseColor.TryParseCssColor(input, out BaseColor? color));

        Assert.IsType(expected, color);
    }

    #endregion -- Parsing --
}
