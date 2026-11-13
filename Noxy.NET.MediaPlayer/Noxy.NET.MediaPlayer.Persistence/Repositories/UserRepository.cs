using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class UserRepository(DataContext context) : IUserRepository
{
    public Task<bool> EmailExists(string email, CancellationToken cancellationToken = default)
    {
        return context.User.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> FindByID(Guid id, CancellationToken cancellationToken = default)
    {
        TableUser? table = await context.User.AsNoTracking().FirstOrDefaultAsync(x => x.ID == id, cancellationToken);
        return table?.ToEntity();
    }

    public async Task<User?> FindByEmail(string email, CancellationToken cancellationToken = default)
    {
        TableUser? table = await context.User.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        return table?.ToEntity();
    }

    public void Add(User user)
    {
        context.User.Add(user.ToTable());
    }
}
