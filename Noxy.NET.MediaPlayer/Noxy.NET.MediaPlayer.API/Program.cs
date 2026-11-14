using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.API.Configurations;
using Noxy.NET.MediaPlayer.API.Handlers;
using Noxy.NET.MediaPlayer.API.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Each appsettings section the API reads is bound to a config class and validated at startup.
builder.Services.AddOptions<ConfigConnectionString>().BindConfiguration(ConfigConnectionString.SectionName).ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddPersistence((services, options) => options.UseSqlite(services.GetRequiredService<IOptions<ConfigConnectionString>>().Value.Database));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddApiCors();
builder.Services.AddApiRateLimiting();
builder.Services.AddApiAuthentication();
builder.Services.AddApiSignalR();
builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseExceptionHandler();
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
app.MapHub<MediaHub>(Constants.MediaHubPath);

await app.RunAsync();

public partial class Program;
