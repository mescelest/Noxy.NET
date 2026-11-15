using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class SettingDeclarationBooleanTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private readonly SettingDeclarationBoolean _declaration = new("Flag", false);

    private static JsonElement Json(string text)
    {
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void ParseValue_AcceptsABooleanOnly(string json, bool expected)
    {
        Assert.Equal(expected, _declaration.ParseValue(Json(json)));
    }

    [Theory]
    [InlineData("\"true\"")]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("\"\"")]
    public void ParseValue_RejectsAnythingElse(string json)
    {
        InvalidSettingValueException exception = Assert.Throws<InvalidSettingValueException>(() => _declaration.ParseValue(Json(json)));

        Assert.Equal("Flag", exception.Key);
    }

    [Fact]
    public void ReadValue_GivesTheDefaultForNothingOrARowOfAnotherType()
    {
        SettingDeclarationBoolean onByDefault = new("Flag", true);

        Assert.Equal(true, onByDefault.ReadValue(null));
        Assert.Equal(true, onByDefault.ReadValue(new EntityUserSettingInteger { UserID = UserID, Key = "Flag", Value = 5 }));
        Assert.Equal(true, onByDefault.ReadValue(new EntityUserSettingEnum { UserID = UserID, Key = "Flag", Value = "x" }));
    }

    [Fact]
    public void ReadValue_GivesTheSavedValue()
    {
        Assert.Equal(true, _declaration.ReadValue(new EntityUserSettingBoolean { UserID = UserID, Key = "Flag", Value = true }));
    }

    [Fact]
    public void CreateEntity_GivesABooleanRowForTheUserAndKey()
    {
        EntityUserSettingBoolean entity = Assert.IsType<EntityUserSettingBoolean>(_declaration.CreateEntity(UserID, true));

        Assert.Equal(UserID, entity.UserID);
        Assert.Equal("Flag", entity.Key);
        Assert.True(entity.Value);
    }

    [Fact]
    public void SetValue_ReportsWhetherItChanged()
    {
        EntityUserSettingBoolean entity = new() { UserID = UserID, Key = "Flag", Value = false };

        Assert.False(_declaration.SetValue(entity, false));
        Assert.True(_declaration.SetValue(entity, true));
        Assert.True(entity.Value);
    }

    [Fact]
    public void SetValue_OnARowOfAnotherType_IsABug()
    {
        Assert.Throws<InvalidOperationException>(() => _declaration.SetValue(new EntityUserSettingInteger { UserID = UserID, Key = "Flag", Value = 1 }, true));
    }
}
