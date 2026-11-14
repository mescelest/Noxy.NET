using Noxy.NET.MediaPlayer.Persistence.Mappers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Persistence;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public sealed class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<DataContext> _options;

    public DatabaseFixture()
    {
        _connection.Open();

        _options = new DbContextOptionsBuilder<DataContext>().UseSqlite(_connection).Options;
        using (DataContext context = new(_options))
        {
            context.Database.EnsureCreated();
        }

        ConfigJwt config = new()
        {
            Secret = new('x', 64),
            Issuer = "test",
            Audience = "test",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 30
        };

        Time = new();
        GoogleTokenService = new();
        UoWFactory = new UnitOfWorkFactory(new ContextFactory(_options), new EntityToTableMapper(), new TableToEntityMapper());
        TokenService = new(Options.Create(config), Time);
        AuthenticationService = new(UoWFactory, new PasswordHashService(), TokenService, GoogleTokenService, Time);
    }

    public FakeTimeProvider Time { get; }
    public FakeGoogleTokenService GoogleTokenService { get; }
    public IUnitOfWorkFactory UoWFactory { get; }
    public TokenService TokenService { get; }
    public AuthenticationService AuthenticationService { get; }

    // A separate context, for looking at the tables directly.
    public DataContext CreateContext()
    {
        return new(_options);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private sealed class ContextFactory(DbContextOptions<DataContext> options) : IDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext()
        {
            return new(options);
        }
    }
}
