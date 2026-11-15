using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Setting;

namespace Noxy.NET.MediaPlayer.Application.Queries.Setting;

public record QuerySettingList(Guid UserID, string? IfNoneMatch) : IQuery<SettingLibrary>;
