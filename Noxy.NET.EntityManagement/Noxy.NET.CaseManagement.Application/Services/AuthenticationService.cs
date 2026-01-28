using Noxy.NET.CaseManagement.Application.Interfaces;
using Noxy.NET.CaseManagement.Application.Interfaces.Services;
using Noxy.NET.CaseManagement.Domain.Entities.Authentication;
using Noxy.NET.CaseManagement.Domain.Forms.Authentication;

namespace Noxy.NET.CaseManagement.Application.Services;

public class AuthenticationService(IUnitOfWorkFactory serviceUoWFactory, IJWTService serviceJWT) : IAuthenticationService
{
    public async Task<string> SignInUser(AuthenticationSignInAPIFormModel model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntityUser entityUser =  await uow.Authentication.GetUserWithEmailAndPassword(model.Email, model.Password);
        return serviceJWT.Create(entityUser);
    }

    public async Task<string> SignUpUser(AuthenticationSignUpAPIFormModel model)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();
        EntityUser entityUser = await uow.Authentication.CreateUser(model.Email, model.Password);
        await uow.Commit();
        return serviceJWT.Create(entityUser);
    }

    public async Task<string> RenewUser(string email)
    {
        await using IUnitOfWork uow = await serviceUoWFactory.Create();

        EntityUser entityUser = await uow.Authentication.GetUserWithEmail(email);
        entityUser.TimeSignIn = DateTime.UtcNow;

        uow.Authentication.UpdateUser(entityUser);
        await uow.Commit();
        
        return serviceJWT.Create(entityUser);
    }
}
