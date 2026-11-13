using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

public class ResponseAuthentication : BaseResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
