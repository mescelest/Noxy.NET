using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository
{
    Task<bool> EmailExists(string email, CancellationToken cancellationToken = default);
    Task<User?> FindByID(Guid id, CancellationToken cancellationToken = default);
    Task<User?> FindByEmail(string email, CancellationToken cancellationToken = default);
    void Add(User user);
}
