using System.Text;
using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Commands.Authentication;

public sealed record CommandAuthenticationRefresh(string RefreshToken) : ICommand<ResponseAuthentication>
{
    private bool PrintMembers(StringBuilder builder) => false;
}
