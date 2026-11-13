using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository
{
    Task<bool> EmailExists(string email, CancellationToken cancellationToken = default);
    void Add(User user);
}
