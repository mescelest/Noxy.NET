using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Setting;

public record NotificationSettingUpdated(Guid UserID, ResponseSetting Setting) : INotification;
