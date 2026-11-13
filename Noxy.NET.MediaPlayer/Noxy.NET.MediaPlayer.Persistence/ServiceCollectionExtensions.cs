using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.MediaPlayer.Persistence.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> options)
    {
        services.AddDbContextFactory<DataContext>(options);
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();

        return services;
    }
}
