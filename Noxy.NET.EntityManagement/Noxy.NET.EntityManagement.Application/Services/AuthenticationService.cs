using Noxy.NET.EntityManagement.Application.Interfaces;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Application.Services;

public class AuthenticationService(IUnitOfWorkFactory serviceUoWFactory, IJWTService serviceJWT) : IAuthenticationService
{
    public async Task<AuthenticationSignInResponse> SignInUser(AuthenticationSignInFormModel model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntityUser entityUser = await uow.Authentication.GetUserWithEmailAndPassword(model.Email, model.Password);
        return new() { JWT = serviceJWT.Create(entityUser) };
    }

    public async Task<AuthenticationSignUpResponse> SignUpUser(AuthenticationSignUpFormModel model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntityUser entityUser = await uow.Authentication.CreateUser(model.Email, model.Password);
        await uow.Commit();
        return new() { JWT = serviceJWT.Create(entityUser) };
    }

    public async Task<AuthenticationRenewResponse> RenewUser(string email)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityUser entityUser = await uow.Authentication.GetUserWithEmail(email);
        entityUser.TimeSignIn = DateTime.UtcNow;

        uow.Authentication.UpdateUser(entityUser);
        await uow.Commit();

        return new() { JWT = serviceJWT.Create(entityUser) };
    }
}
