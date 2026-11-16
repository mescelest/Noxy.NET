using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.API.Configurations;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Development validates the container when the host is built; production must too, so a handler that cannot be constructed stops the start-up instead of failing the first request that publishes after its commit.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

builder.Services.AddOpenApi();

// Each appsettings section the API reads is bound to a config class and validated at startup.
builder.Services.AddOptions<ConfigConnectionString>().BindConfiguration(ConfigConnectionString.SectionName).ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Singleton);
builder.Services.AddPersistence((services, options) => options.UseSqlite(services.GetRequiredService<IOptions<ConfigConnectionString>>().Value.Database));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddApiCors();
builder.Services.AddApiRateLimiting();
builder.Services.AddApiAuthentication();
builder.Services.AddApiSignalR();
builder.Services.AddApiResponseCompression();
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseResponseCompression();
app.UseCors(Constants.CorsPolicyName);
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MediaHub>(HubConstants.MediaHubPath);

await app.RunAsync();

public partial class Program;
