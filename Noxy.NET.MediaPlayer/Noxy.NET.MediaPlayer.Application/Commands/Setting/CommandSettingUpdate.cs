using System.Text.Json;
using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.Application.Commands.Setting;

public record CommandSettingUpdate(Guid UserID, string Key, JsonElement Value) : ICommand<ResponseSetting>;
