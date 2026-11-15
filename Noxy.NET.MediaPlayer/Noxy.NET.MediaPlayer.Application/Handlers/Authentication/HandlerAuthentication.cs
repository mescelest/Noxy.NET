using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Authentication;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Authentication;

public abstract class HandlerAuthentication<TCommand>(IUnitOfWorkFactory factoryUnitOfWork, ITokenService serviceToken) : ICommandHandler<TCommand, ResponseAuthentication> where TCommand : ICommand<ResponseAuthentication>
{
    protected IUnitOfWorkFactory FactoryUnitOfWork { get; } = factoryUnitOfWork;

    protected ITokenService ServiceToken { get; } = serviceToken;

    public abstract ValueTask<ResponseAuthentication> Handle(TCommand command, CancellationToken cancellationToken);

    protected ResponseAuthentication CreateTokenResponse(IUnitOfWork uow, EntityUser user)
    {
        AccessToken accessToken = ServiceToken.CreateAccessToken(user);
        RefreshToken refreshToken = ServiceToken.CreateRefreshToken();

        uow.Authentication.AddRefreshToken(new()
        {
            UserID = user.ID,
            TokenHash = refreshToken.Hash,
            TimeExpires = refreshToken.TimeExpires
        });

        return new()
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken.Token
        };
    }

    protected static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
