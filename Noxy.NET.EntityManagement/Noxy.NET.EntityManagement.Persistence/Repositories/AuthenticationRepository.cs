using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Noxy.NET.EntityManagement.Application.Interfaces.Repositories;
using Noxy.NET.EntityManagement.Application.Interfaces.Services;
using Noxy.NET.EntityManagement.Domain.Entities.Authentication;
using Noxy.NET.EntityManagement.Persistence.Abstractions;
using Noxy.NET.EntityManagement.Persistence.Tables.Authentication;

namespace Noxy.NET.EntityManagement.Persistence.Repositories;

public class AuthenticationRepository(DataContext context, IDependencyInjectionService serviceDependencyInjection) : BaseRepository(context, serviceDependencyInjection), IAuthenticationRepository
{
    private static int KeySize => 64;
    private static int HashIterations => 64;
    private static HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA512;

    public async Task<EntityUser> GetUserWithID(Guid id)
    {
        TableUser tableUser = await Context.User.AsNoTracking().FirstAsync(x => x.ID == id);
        return MapperT2E.Map(tableUser);
    }

    public async Task<EntityUser> GetUserWithEmail(string email)
    {
        TableUser tableUser = await Context.User.AsNoTracking().FirstAsync(x => x.Email == email);
        return MapperT2E.Map(tableUser);
    }

    public async Task<EntityUser> GetUserWithEmailAndPassword(string email, string password)
    {
        TableUser tableUser = await Context.User.AsNoTracking().Include(x => x.Authentication).FirstAsync(x => x.Email == email);
        if (tableUser.Authentication == null)
        {
            throw new();
        }

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), tableUser.Authentication.Salt, HashIterations, HashAlgorithm, KeySize);
        return CryptographicOperations.FixedTimeEquals(tableUser.Authentication.Hash, hash) ? MapperT2E.Map(tableUser) : throw new();
    }

    public async Task<EntityUser> CreateUser(string email, string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(KeySize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, HashIterations, HashAlgorithm, KeySize);

        EntityEntry<TableAuthentication> entryAuthentication = await Context.Authentication.AddAsync(new()
        {
            Salt = salt,
            Hash = hash
        });

        EntityEntry<TableUser> entryUser = await Context.User.AddAsync(new()
        {
            Email = email,
            AuthenticationID = entryAuthentication.Entity.ID
        });

        return MapperT2E.Map(entryUser.Entity);
    }

    public void UpdateUser(EntityUser entityUser)
    {
        Context.User.Update(MapperE2T.Map(entityUser));
    }

    public async Task<List<EntityIdentity>> GetIdentityListWithUserID(Guid idUser)
    {
        List<TableIdentity> list = await Context.Identity
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.UserID == idUser)
            .ToListAsync();

        return list.Select(MapperT2E.Map).ToList();
    }

    public async Task<EntityIdentity> GetIdentityWithID(Guid idIdentity)
    {
        TableIdentity tableIdentity = await Context.Identity
            .AsNoTracking()
            .AsSplitQuery()
            .SingleAsync(x => x.ID == idIdentity);

        return MapperT2E.Map(tableIdentity);
    }
}
