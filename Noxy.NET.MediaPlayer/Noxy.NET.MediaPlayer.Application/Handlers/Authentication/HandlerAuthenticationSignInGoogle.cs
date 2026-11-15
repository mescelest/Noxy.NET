using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Authentication;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Authentication;

public class HandlerAuthenticationSignInGoogle(IUnitOfWorkFactory factoryUnitOfWork, IGoogleTokenService serviceGoogleToken, ITokenService serviceToken) : HandlerAuthentication<CommandAuthenticationSignInGoogle>(factoryUnitOfWork, serviceToken)
{
    public override async ValueTask<ResponseAuthentication> Handle(CommandAuthenticationSignInGoogle command, CancellationToken cancellationToken)
    {
        GoogleIdentity identity = await serviceGoogleToken.Validate(command.IdToken);

        await using IUnitOfWork uow = FactoryUnitOfWork.Create();

        // Google's user ID is the identity, not the email: emails can change, and can be reused by someone else.
        EntityExternalLogin? login = await uow.Authentication.FindExternalLogin(ExternalLoginProvider.Google, identity.Subject, cancellationToken);

        EntityUser user;
        if (login is not null)
        {
            user = await uow.Authentication.FindUserByID(login.UserID, cancellationToken) ?? throw new AuthenticationFailedException();
        }
        else
        {
            // First time with this Google account. Creating or linking an account claims an email address,
            // so Google has to have verified that address.
            if (!identity.IsEmailVerified)
            {
                throw new AuthenticationFailedException();
            }

            string email = NormalizeEmail(identity.Email);
            EntityUser? existing = await uow.Authentication.FindUserByEmail(email, cancellationToken);

            if (existing is null)
            {
                user = new() { Email = email, IsEmailVerified = true };
                user.ExternalLoginList.Add(new() { Provider = ExternalLoginProvider.Google, ProviderKey = identity.Subject, UserID = user.ID });
                uow.Authentication.AddUser(user);
            }
            else if (existing.IsEmailVerified)
            {
                // Both sides have proven they own the address, so it's safe to attach Google to the existing account.
                user = existing;
                uow.Authentication.AddExternalLogin(new() { Provider = ExternalLoginProvider.Google, ProviderKey = identity.Subject, UserID = user.ID });
            }
            else
            {
                // The existing account's email was never verified (anyone can register an address with a password),
                // so linking could hand the account to whoever registered it first.
                throw new EmailAlreadyInUseException();
            }
        }

        ResponseAuthentication response = CreateTokenResponse(uow, user);

        await uow.Commit(cancellationToken);
        return response;
    }
}
