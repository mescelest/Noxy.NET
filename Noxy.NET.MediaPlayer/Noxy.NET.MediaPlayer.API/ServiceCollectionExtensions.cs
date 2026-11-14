using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.API.Configurations;
using Noxy.NET.MediaPlayer.Application.Configurations;

#pragma warning disable IDE0130, S1200
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiCors()
        {
            services.AddOptions<ConfigCors>().BindConfiguration(ConfigCors.SectionName).ValidateDataAnnotations().ValidateOnStart();

            services.AddCors();
            services.AddOptions<CorsOptions>()
                .Configure<IOptions<ConfigCors>>((cors, settings) => cors.AddPolicy(Constants.CorsPolicyName, policy => policy.WithOrigins(settings.Value.Origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

            return services;
        }

        public IServiceCollection AddApiRateLimiting()
        {
            services.AddOptions<ConfigRateLimitAuth>().BindConfiguration(ConfigRateLimitAuth.SectionName).ValidateDataAnnotations().ValidateOnStart();

            services.AddRateLimiter(options =>
            {
                options.AddPolicy(Constants.RateLimitPolicyAuth, context =>
                {
                    ConfigRateLimitAuth settings = context.RequestServices.GetRequiredService<IOptions<ConfigRateLimitAuth>>().Value;

                    return RateLimitPartition.GetSlidingWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new()
                    {
                        PermitLimit = settings.PermitLimit,
                        Window = settings.Window,
                        SegmentsPerWindow = 6,
                        QueueLimit = 0
                    });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    HttpResponse response = context.HttpContext.Response;
                    response.StatusCode = StatusCodes.Status429TooManyRequests;

                    // The sliding window can't say when a permit frees up, so give the full window: always safe, sometimes pessimistic.
                    ConfigRateLimitAuth settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<ConfigRateLimitAuth>>().Value;
                    response.Headers.RetryAfter = settings.WindowSeconds.ToString(CultureInfo.InvariantCulture);

                    await response.WriteAsJsonAsync(new ProblemDetails { Status = response.StatusCode, Title = "Too many requests." }, cancellationToken);
                };
            });

            return services;
        }

        public IServiceCollection AddApiAuthentication()
        {
            // Validation uses the same ConfigJwt that TokenService signs with, so the two can't drift apart.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<ConfigJwt>>((bearer, jwt) =>
                {
                    bearer.MapInboundClaims = false; // keep claim names as issued ("sub", "email")
                    bearer.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Value.Issuer,
                        ValidAudience = jwt.Value.Audience,
                        IssuerSigningKey = jwt.Value.CreateSigningKey(),
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });

            // Secure by default: any endpoint without its own authorization metadata requires a signed-in user.
            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

            return services;
        }
    }
}
