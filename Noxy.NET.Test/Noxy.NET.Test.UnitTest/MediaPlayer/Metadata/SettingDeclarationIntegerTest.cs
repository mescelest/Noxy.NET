using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class SettingDeclarationIntegerTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private readonly SettingDeclarationInteger _declaration = new("Count", 3, 0, 10);

    private static JsonElement Json(string text)
    {
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("3", 3)]
    [InlineData("10", 10)]
    public void ParseValue_AcceptsAWholeNumberFromTheMinimumToTheMaximum(string json, int expected)
    {
        Assert.Equal(expected, _declaration.ParseValue(Json(json)));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-2147483648")]
    [InlineData("11")]
    [InlineData("20")]
    [InlineData("2147483647")]
    [InlineData("2147483648")]
    [InlineData("99999999999999999999")]
    [InlineData("2.5")]
    [InlineData("\"5\"")]
    [InlineData("true")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    public void ParseValue_RejectsAnythingOutsideTheRangeOrNotAWholeNumber(string json)
    {
        InvalidSettingValueException exception = Assert.Throws<InvalidSettingValueException>(() => _declaration.ParseValue(Json(json)));

        Assert.Equal("Count", exception.Key);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(int.MaxValue)]
    public void ReadValue_GivesTheDefaultForASavedValueOutsideTheRange(int saved)
    {
        Assert.Equal(3, _declaration.ReadValue(new EntityUserSettingInteger { UserID = UserID, Key = "Count", Value = saved }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void ReadValue_GivesTheSavedValueAtTheEdgesOfTheRange(int saved)
    {
        Assert.Equal(saved, _declaration.ReadValue(new EntityUserSettingInteger { UserID = UserID, Key = "Count", Value = saved }));
    }

    [Fact]
    public void ReadValue_GivesTheDefaultForNothingOrARowOfAnotherType()
    {
        Assert.Equal(3, _declaration.ReadValue(null));
        Assert.Equal(3, _declaration.ReadValue(new EntityUserSettingBoolean { UserID = UserID, Key = "Count", Value = true }));
    }
}
