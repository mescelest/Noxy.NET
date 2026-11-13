using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Requests.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class AuthenticationService(IUnitOfWorkFactory uowFactory, IPasswordHashService passwordHashService) : IAuthenticationService
{
    public async Task SignUp(RequestSignUp request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        string email = NormalizeEmail(request.Email);
        if (await uow.Users.EmailExists(email, cancellationToken))
        {
            throw new EmailAlreadyInUseException();
        }

        uow.Users.Add(new()
        {
            Email = email,
            PasswordHash = passwordHashService.Hash(request.Password)
        });

        await uow.Commit(cancellationToken);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
