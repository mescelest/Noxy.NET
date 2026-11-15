using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.Application.Models.Setting;

public record SettingLibrary(string ETag, List<ResponseSetting>? SettingList);
