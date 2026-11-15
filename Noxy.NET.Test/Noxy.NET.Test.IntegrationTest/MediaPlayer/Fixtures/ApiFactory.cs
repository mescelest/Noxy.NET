using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Noxy.NET.MediaPlayer.API.Configurations;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.MediaPlayer.Persistence.Services;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly Dictionary<string, string?> _settings;

    public ApiFactory(Dictionary<string, string?>? overrides = null)
    {
        Directory.CreateDirectory(_directory);
        string connectionString = $"Data Source={Path.Combine(_directory, "test.sqlite")}";

        _settings = new()
        {
            [ConfigurationPath.Combine(ConfigConnectionString.SectionName, nameof(ConfigConnectionString.Database))] = connectionString,
            [ConfigurationPath.Combine(ConfigJwt.SectionName, nameof(ConfigJwt.Secret))] = new string('s', 48),
            [ConfigurationPath.Combine(ConfigStorage.SectionName, nameof(ConfigStorage.RootPath))] = StorageRoot,
            [ConfigurationPath.Combine(ConfigStorage.SectionName, nameof(ConfigStorage.MaxSizeBytes))] = "100000",
            [ConfigurationPath.Combine(ConfigUpload.SectionName, nameof(ConfigUpload.PollSeconds))] = "3600"
        };
        foreach ((string key, string? value) in overrides ?? [])
        {
            _settings[key] = value;
        }

        // The schema comes from the real migrations, and exists before the host (and its worker) starts.
        DbContextOptions<DataContext> options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connectionString).Options;
        using DataContext context = new(options);
        context.Database.Migrate();
    }

    public string StorageRoot => Path.Combine(_directory, "media");
    public string StagingFolder => Path.Combine(StorageRoot, FileStorageService.StagingFolderName);
    public string TemporaryFolder => Path.Combine(StorageRoot, FileStorageService.TemporaryFolderName);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(_settings));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;

        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(_directory, true);
        }
        catch (IOException)
        {
            // A temp folder that is still in use is not worth failing a test for.
        }
    }
}
