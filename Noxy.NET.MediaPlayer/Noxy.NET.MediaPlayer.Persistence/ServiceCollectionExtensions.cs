using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Services;
using Noxy.NET.MediaPlayer.Persistence.Services;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPersistence(Action<IServiceProvider, DbContextOptionsBuilder> options)
        {
            services.AddDbContextFactory<DataContext>(options);
            services.AddSingleton<IEntityToTableMapper, EntityToTableMapper>();
            services.AddSingleton<ITableToEntityMapper, TableToEntityMapper>();
            services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
            services.AddSingleton<IFileStorageService, FileStorageService>();

            return services;
        }
    }
}
