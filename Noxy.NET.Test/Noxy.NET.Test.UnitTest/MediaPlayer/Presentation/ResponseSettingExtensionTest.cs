using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Extensions;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ResponseSettingExtensionTest
{
    private static ResponseSetting Setting(object value, string key = "Key")
    {
        return new() { Key = key, ValueType = SettingValueTypeEnum.Boolean, Value = JsonSerializer.SerializeToElement(value), TimeModified = DateTime.UtcNow };
    }

    [Fact]
    public void ABoolean_AnInteger_AndAnEnumName_AreReadByTheirKind()
    {
        Assert.True(Setting(true).GetBoolean());
        Assert.False(Setting(false).GetBoolean());
        Assert.Equal(40, Setting(40).GetInteger());
        Assert.Equal("Forever", Setting("Forever").GetEnumName());
    }

    [Fact]
    public void AValueOfAnotherKind_ThrowsInvalidOperation_NamingTheKeyButNotTheValue()
    {
        ResponseSetting text = Setting("secret-value", "Repeat");

        InvalidOperationException boolean = Assert.Throws<InvalidOperationException>(() => text.GetBoolean());
        InvalidOperationException integer = Assert.Throws<InvalidOperationException>(() => text.GetInteger());
        InvalidOperationException name = Assert.Throws<InvalidOperationException>(() => Setting(true, "Volume").GetEnumName());

        Assert.Contains("Repeat", boolean.Message);
        Assert.DoesNotContain("secret-value", boolean.Message);
        Assert.Contains("Repeat", integer.Message);
        Assert.Contains("Volume", name.Message);
    }

    [Fact]
    public void AnIntegerWithADecimalPart_IsNotAnInteger()
    {
        Assert.Throws<InvalidOperationException>(() => Setting(3.5, "Volume").GetInteger());
    }
}
