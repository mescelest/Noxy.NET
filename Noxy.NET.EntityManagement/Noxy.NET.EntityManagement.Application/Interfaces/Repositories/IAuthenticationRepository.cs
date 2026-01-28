using Noxy.NET.EntityManagement.Domain.Entities.Authentication;

namespace Noxy.NET.EntityManagement.Application.Interfaces.Repositories;

public interface IAuthenticationRepository
{
    public Task<EntityUser> GetUserWithID(Guid id);
    public Task<EntityUser> GetUserWithEmail(string email);
    public Task<EntityUser> GetUserWithEmailAndPassword(string email, string password);
    public Task<EntityUser> CreateUser(string email, string password);
    public void UpdateUser(EntityUser entityUser);
}
