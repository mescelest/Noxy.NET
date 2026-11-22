using Noxy.NET.UI.Components.Elements;

namespace Noxy.NET.Test.Shared;

public class SeekBarTest
{
    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(50, 100, 50)]
    [InlineData(100, 100, 100)]
    [InlineData(1, 3, 33.33)]
    public void ProgressPercent_IsTheShareOfMax(double value, double max, double expected)
    {
        Assert.Equal(expected, SeekBar.ProgressPercent(value, max), 2);
    }

    [Theory]
    [InlineData(-5, 100, 0)]
    [InlineData(150, 100, 100)]
    [InlineData(10, 0, 0)]
    [InlineData(10, -3, 0)]
    public void ProgressPercent_StaysBetweenZeroAndAHundred_AndIsZeroWithoutALength(double value, double max, double expected)
    {
        Assert.Equal(expected, SeekBar.ProgressPercent(value, max));
    }
}
