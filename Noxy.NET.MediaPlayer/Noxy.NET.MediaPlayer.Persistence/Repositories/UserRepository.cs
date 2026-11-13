using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Mappers;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class UserRepository(DataContext context) : IUserRepository
{
    public Task<bool> EmailExists(string email, CancellationToken cancellationToken = default)
    {
        return context.User.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public void Add(User user)
    {
        context.User.Add(user.ToTable());
    }
}
