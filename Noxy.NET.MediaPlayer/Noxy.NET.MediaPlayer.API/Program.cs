using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

const string corsPolicyName = "Noxy.NET-CORS-Policy";
string[] corsOrigins = builder.Configuration.GetSection("CORS:Origins").Get<string[]>() ?? [];

builder.Services.AddCors(options => options.AddPolicy(corsPolicyName, policy => policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddApplication();
builder.Services.AddPersistence(x => x.UseSqlite(@"Data Source=..\..\Data\Noxy.NET.MediaPlayer.sqlite"));

builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseCors(corsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
