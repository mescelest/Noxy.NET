using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Persistence.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class ExternalLoginRepository(DataContext context) : IExternalLoginRepository
{
    public async Task<ExternalLogin?> FindByProviderKey(ExternalLoginProvider provider, string providerKey, CancellationToken cancellationToken = default)
    {
        TableExternalLogin? table = await context.ExternalLogin.AsNoTracking().FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey, cancellationToken);
        return table?.ToEntity();
    }

    public void Add(ExternalLogin externalLogin)
    {
        context.ExternalLogin.Add(externalLogin.ToTable());
    }
}
