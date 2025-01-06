using System.Security.Claims;
using System.Text;
using Azure.Security.KeyVault.Secrets;
using Domain.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence.Context;

namespace Infrastructure.Configuration;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Identity yapılandırması
        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<ErpaKabloDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager<SignInManager<AppUser>>();
        
        services.AddSingleton<SymmetricSecurityKey>(serviceProvider =>
        {
            // Get the SecretClient that was registered in your SecurityServiceRegistration
            var secretClient = serviceProvider.GetRequiredService<SecretClient>();
            // Get the key synchronously during startup
            var jwtKey = secretClient.GetSecret("JwtSecurityKey").Value;
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey.Value));
        });

        // JWT ve cookie tabanlı kimlik doğrulama tek bir yerde yapılandırılıyor
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("Admin", options =>  // Admin şemasını koruyoruz
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    ValidIssuer = configuration["Security:Token:Issuer"],
                    ValidAudience = configuration["Security:Token:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            configuration["Security:Token:SecurityKey"]
                            ?? throw new InvalidOperationException("JWT security key not found"))),
                    LifetimeValidator = (notBefore, expires, token, parameters) => 
                        expires != null && expires > DateTime.UtcNow,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                
                        if (!string.IsNullOrEmpty(accessToken) && 
                            path.StartsWithSegments("/order-hub"))
                        {
                            context.Token = accessToken;
                        }
                
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}