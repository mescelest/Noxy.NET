using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

const string corsPolicyName = "Noxy.NET-CORS-Policy";
string[] corsOrigins = builder.Configuration.GetSection("CORS:Origins").Get<string[]>() ?? [];

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options => options.AddPolicy(corsPolicyName, policy => policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddApplication();
builder.Services.AddPersistence(x => x.UseSqlite($@"Data Source=..\..\Data\Noxy.NET.EntityManagement.sqlite"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        string secret = builder.Configuration["Authentication:Secret"] ?? throw new KeyNotFoundException("Authentication:Secret");
        opts.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"] ?? throw new KeyNotFoundException("Authentication:Issuer"),
            ValidAudience = builder.Configuration["Authentication:Audience"] ?? throw new KeyNotFoundException("Authentication:Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

await app.UseBaseWithPersistence();
await app.UseApplication();

await app.RunAsync();
