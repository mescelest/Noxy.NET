using System.Text.Json.Serialization;
using Noxy.NET.MediaPlayer.Domain.Abstractions;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Setting;

public class ResponseSetting : BaseResponse
{
    public required string Key { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required SettingValueType ValueType { get; init; }

    // A bool, an int or the name of an enum member, by the type of the setting.
    public required object Value { get; init; }

    public required DateTime TimeModified { get; init; }
}
