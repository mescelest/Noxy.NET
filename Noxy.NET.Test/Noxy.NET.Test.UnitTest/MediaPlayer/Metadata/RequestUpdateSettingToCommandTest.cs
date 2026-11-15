using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Commands.Setting;
using Noxy.NET.MediaPlayer.Application.Requests.Setting;

namespace Noxy.NET.Test.MediaPlayer.Metadata;

public class RequestUpdateSettingToCommandTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    [Fact]
    public void ToCommand_CarriesTheUserTheKeyAndTheValue()
    {
        JsonElement value = JsonDocument.Parse("40").RootElement.Clone();

        CommandSettingUpdate command = new RequestUpdateSetting { Value = value }.ToCommand(UserID, "Volume");

        Assert.Equal(UserID, command.UserID);
        Assert.Equal("Volume", command.Key);
        Assert.Equal(40, command.Value.GetInt32());
    }

    [Fact]
    public void TheRequestJson_BindsTheValueOfAnyJsonKind()
    {
        RequestUpdateSetting number = JsonSerializer.Deserialize<RequestUpdateSetting>("""{"value":5}""", JsonSerializerOptions.Web) ?? throw new InvalidOperationException();
        RequestUpdateSetting text = JsonSerializer.Deserialize<RequestUpdateSetting>("""{"value":"Forever"}""", JsonSerializerOptions.Web) ?? throw new InvalidOperationException();
        RequestUpdateSetting boolean = JsonSerializer.Deserialize<RequestUpdateSetting>("""{"value":true}""", JsonSerializerOptions.Web) ?? throw new InvalidOperationException();
        RequestUpdateSetting nothing = JsonSerializer.Deserialize<RequestUpdateSetting>("""{"value":null}""", JsonSerializerOptions.Web) ?? throw new InvalidOperationException();

        Assert.Equal(JsonValueKind.Number, number.Value.ValueKind);
        Assert.Equal(JsonValueKind.String, text.Value.ValueKind);
        Assert.Equal(JsonValueKind.True, boolean.Value.ValueKind);
        Assert.Equal(JsonValueKind.Null, nothing.Value.ValueKind);
    }

    [Fact]
    public void ABodyWithNoValue_CannotBeBound()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<RequestUpdateSetting>("{}", JsonSerializerOptions.Web));
    }
}
