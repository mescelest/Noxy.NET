using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class SettingDeclarationIntegerTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private readonly SettingDeclarationInteger _declaration = new("SampleInteger", 100, 0, 100);

    private static JsonElement Json(string text)
    {
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("50", 50)]
    [InlineData("100", 100)]
    public void ParseValue_AcceptsAWholeNumberInsideTheLimits(string json, int expected)
    {
        Assert.Equal(expected, _declaration.ParseValue(Json(json)));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("3.5")]
    [InlineData("1e2")]
    [InlineData("99999999999")]
    [InlineData("\"50\"")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("{}")]
    public void ParseValue_RejectsAnythingElse(string json)
    {
        InvalidSettingValueException exception = Assert.Throws<InvalidSettingValueException>(() => _declaration.ParseValue(Json(json)));

        Assert.Equal(_declaration.Key, exception.Key);
    }

    [Fact]
    public void ReadValue_GivesTheDefaultForNothingARowOfAnotherTypeOrANumberOutsideTheLimits()
    {
        Assert.Equal(100, _declaration.ReadValue(null));
        Assert.Equal(100, _declaration.ReadValue(new EntityUserSettingBoolean { UserID = UserID, Key = _declaration.Key, Value = true }));
        Assert.Equal(100, _declaration.ReadValue(new EntityUserSettingInteger { UserID = UserID, Key = _declaration.Key, Value = 200 }));
        Assert.Equal(100, _declaration.ReadValue(new EntityUserSettingInteger { UserID = UserID, Key = _declaration.Key, Value = -5 }));
    }

    [Fact]
    public void ReadValue_GivesTheSavedValue()
    {
        Assert.Equal(40, _declaration.ReadValue(new EntityUserSettingInteger { UserID = UserID, Key = _declaration.Key, Value = 40 }));
    }

    [Fact]
    public void CreateEntity_GivesAnIntegerRowForTheUserAndKey()
    {
        EntityUserSettingInteger entity = Assert.IsType<EntityUserSettingInteger>(_declaration.CreateEntity(UserID, 30));

        Assert.Equal(UserID, entity.UserID);
        Assert.Equal(_declaration.Key, entity.Key);
        Assert.Equal(30, entity.Value);
    }

    [Fact]
    public void SetValue_ReportsWhetherItChanged()
    {
        EntityUserSettingInteger entity = new() { UserID = UserID, Key = _declaration.Key, Value = 30 };

        Assert.False(_declaration.SetValue(entity, 30));
        Assert.True(_declaration.SetValue(entity, 60));
        Assert.Equal(60, entity.Value);
    }
}
