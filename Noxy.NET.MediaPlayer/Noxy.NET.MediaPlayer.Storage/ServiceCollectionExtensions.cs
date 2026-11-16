using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Storage;
using Noxy.NET.MediaPlayer.Storage.Stores;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        // The folder is the app's own data folder; the database file inside it is created on first use.
        public IServiceCollection AddStorage(string dataFolder)
        {
            services.AddSingleton<IMetadataStore>(_ => new SqliteMetadataStore(Path.Combine(dataFolder, Constants.MetadataDatabaseFileName)));
            services.AddSingleton<IFileStore>(_ => new DiskFileStore(dataFolder));

            return services;
        }
    }
}
