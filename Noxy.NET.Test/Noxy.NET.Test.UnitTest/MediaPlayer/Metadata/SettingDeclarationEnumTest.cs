using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class SettingDeclarationEnumTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private readonly SettingDeclarationEnumOf<RepeatModeEnum> _declaration = SettingCatalog.Repeat;

    private static JsonElement Json(string text)
    {
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    [Theory]
    [InlineData("\"No\"")]
    [InlineData("\"Once\"")]
    [InlineData("\"Forever\"")]
    public void ParseValue_AcceptsAMemberName(string json)
    {
        Assert.Equal(JsonSerializer.Deserialize<string>(json), _declaration.ParseValue(Json(json)));
    }

    [Theory]
    [InlineData("\"forever\"")]
    [InlineData("\"FOREVER\"")]
    [InlineData("\"Sometimes\"")]
    [InlineData("\"\"")]
    [InlineData("\" Once\"")]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("[]")]
    public void ParseValue_RejectsAnythingElse_NamesAndNumbersAlike(string json)
    {
        InvalidSettingValueException exception = Assert.Throws<InvalidSettingValueException>(() => _declaration.ParseValue(Json(json)));

        Assert.Equal(SettingKey.Repeat, exception.Key);
    }

    [Fact]
    public void TheMembersAndTheDefaultComeFromTheEnum()
    {
        Assert.Equal(["No", "Once", "Forever"], _declaration.MemberNameList);
        Assert.Equal("No", _declaration.DefaultValue);
        Assert.Equal(SettingValueTypeEnum.Enum, _declaration.ValueType);
        Assert.Equal(RepeatModeEnum.No, _declaration.Default);
    }

    [Fact]
    public void ReadValue_GivesTheDefaultForNothingARowOfAnotherTypeOrAMemberThatWasRemoved()
    {
        Assert.Equal("No", _declaration.ReadValue(null));
        Assert.Equal("No", _declaration.ReadValue(new EntityUserSettingInteger { UserID = UserID, Key = SettingKey.Repeat, Value = 2 }));
        Assert.Equal("No", _declaration.ReadValue(new EntityUserSettingEnum { UserID = UserID, Key = SettingKey.Repeat, Value = "Removed" }));
    }

    [Fact]
    public void ReadValue_GivesTheSavedMember()
    {
        Assert.Equal("Forever", _declaration.ReadValue(new EntityUserSettingEnum { UserID = UserID, Key = SettingKey.Repeat, Value = "Forever" }));
    }

    [Fact]
    public void CreateEntity_GivesAnEnumRowStoringTheMemberName()
    {
        EntityUserSettingEnum entity = Assert.IsType<EntityUserSettingEnum>(_declaration.CreateEntity(UserID, "Once"));

        Assert.Equal(UserID, entity.UserID);
        Assert.Equal(SettingKey.Repeat, entity.Key);
        Assert.Equal("Once", entity.Value);
    }

    [Fact]
    public void SetValue_ReportsWhetherItChanged()
    {
        EntityUserSettingEnum entity = new() { UserID = UserID, Key = SettingKey.Repeat, Value = "Once" };

        Assert.False(_declaration.SetValue(entity, "Once"));
        Assert.True(_declaration.SetValue(entity, "Forever"));
        Assert.Equal("Forever", entity.Value);
    }
}
