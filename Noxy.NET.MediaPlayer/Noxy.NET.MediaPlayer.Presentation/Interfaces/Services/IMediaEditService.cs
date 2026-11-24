using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IMediaEditService
{
    // Sends the fields of the form that differ from the media file, and applies the answer; nothing differing means no call. A refusal or a lost connection is thrown (ApiException, HttpRequestException).
    Task Edit(ResponseMediaFile original, FormModelEditMedia model);
}
