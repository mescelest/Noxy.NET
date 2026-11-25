using System.ComponentModel;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class RowSelectEnumExtensionTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CheckboxOnly_NeverToggles_OnAClickOnTheRow(bool isCtrlPressed)
    {
        Assert.False(RowSelectEnum.CheckboxOnly.IsToggledBy(isCtrlPressed));
    }

    [Fact]
    public void Ctrl_TogglesOnlyWhileCtrlIsHeld()
    {
        Assert.True(RowSelectEnum.Ctrl.IsToggledBy(true));
        Assert.False(RowSelectEnum.Ctrl.IsToggledBy(false));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Click_AlwaysToggles(bool isCtrlPressed)
    {
        Assert.True(RowSelectEnum.Click.IsToggledBy(isCtrlPressed));
    }

    [Fact]
    public void ToText_NamesEachMode_ForTheStylesheet()
    {
        Assert.Equal("checkbox", RowSelectEnum.CheckboxOnly.ToText());
        Assert.Equal("ctrl", RowSelectEnum.Ctrl.ToText());
        Assert.Equal("click", RowSelectEnum.Click.ToText());
    }

    [Fact]
    public void AModeOutsideTheEnum_Throws()
    {
        Assert.Throws<InvalidEnumArgumentException>(() => ((RowSelectEnum)99).ToText());
        Assert.Throws<InvalidEnumArgumentException>(() => ((RowSelectEnum)99).IsToggledBy(true));
    }
}
