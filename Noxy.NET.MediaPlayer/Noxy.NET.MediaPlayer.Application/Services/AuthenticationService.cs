using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Requests.Authentication;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class AuthenticationService(IUnitOfWorkFactory uowFactory, IPasswordHashService passwordHashService, ITokenService tokenService, TimeProvider time) : IAuthenticationService
{
    public async Task<ResponseAuthentication> SignUp(RequestSignUp request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        string email = NormalizeEmail(request.Email);
        if (await uow.Users.EmailExists(email, cancellationToken))
        {
            throw new EmailAlreadyInUseException();
        }

        User user = new()
        {
            Email = email,
            PasswordHash = passwordHashService.Hash(request.Password)
        };
        uow.Users.Add(user);

        ResponseAuthentication response = IssueTokens(uow, user);

        await uow.Commit(cancellationToken);
        return response;
    }

    public async Task<ResponseAuthentication> SignIn(RequestSignIn request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        User? user = await uow.Users.FindByEmail(NormalizeEmail(request.Email), cancellationToken);

        // Verify even when there is no user, so an unknown email takes as long as a wrong password.
        bool isPasswordValid = passwordHashService.Verify(request.Password, user?.PasswordHash);
        if (user == null || !isPasswordValid)
        {
            throw new AuthenticationFailedException();
        }

        ResponseAuthentication response = IssueTokens(uow, user);

        await uow.Commit(cancellationToken);
        return response;
    }

    public async Task<ResponseAuthentication> Refresh(RequestRefresh request, CancellationToken cancellationToken = default)
    {
        DateTime now = time.GetUtcNow().UtcDateTime;

        await using IUnitOfWork uow = uowFactory.Create();

        RefreshToken stored = await uow.RefreshTokens.FindByHash(tokenService.HashRefreshToken(request.RefreshToken), cancellationToken)
                              ?? throw new AuthenticationFailedException();

        // Tokens are single-use, so a revoked one coming back means someone has a copy.
        // Revoke everything this user has and make them sign in again.
        if (stored.IsRevoked)
        {
            await uow.RefreshTokens.RevokeAllForUser(stored.UserID, now, cancellationToken);
            throw new AuthenticationFailedException();
        }

        if (stored.IsExpired(now))
        {
            throw new AuthenticationFailedException();
        }

        // Losing this race to another request using the same token counts as reuse too.
        if (!await uow.RefreshTokens.TryRevoke(stored.ID, now, cancellationToken))
        {
            await uow.RefreshTokens.RevokeAllForUser(stored.UserID, now, cancellationToken);
            throw new AuthenticationFailedException();
        }

        User user = await uow.Users.FindByID(stored.UserID, cancellationToken) ?? throw new AuthenticationFailedException();

        ResponseAuthentication response = IssueTokens(uow, user);

        await uow.Commit(cancellationToken);
        return response;
    }

    private ResponseAuthentication IssueTokens(IUnitOfWork uow, User user)
    {
        AccessToken accessToken = tokenService.CreateAccessToken(user);
        RefreshTokenSecret refreshToken = tokenService.CreateRefreshToken();

        uow.RefreshTokens.Add(new()
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

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
