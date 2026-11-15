using System.Text.Json;
using Noxy.NET.MediaPlayer.Application.Commands.Setting;

namespace Noxy.NET.MediaPlayer.Application.Requests.Setting;

public class RequestUpdateSetting
{
    public required JsonElement Value { get; set; }

    public CommandSettingUpdate ToCommand(Guid userID, string key)
    {
        return new(userID, key, Value);
    }
}
