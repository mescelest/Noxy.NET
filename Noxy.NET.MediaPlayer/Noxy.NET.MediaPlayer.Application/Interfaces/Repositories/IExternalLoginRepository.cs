using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IExternalLoginRepository : IRepository
{
    Task<ExternalLogin?> FindByProviderKey(ExternalLoginProvider provider, string providerKey, CancellationToken cancellationToken = default);
    void Add(ExternalLogin externalLogin);
}
