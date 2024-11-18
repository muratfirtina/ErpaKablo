using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Application.Tokens;
using Domain.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Token;

public class TokenHandler : ITokenHandler
{
    private readonly IConfiguration _configuration;

    public TokenHandler(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public Application.Dtos.Token.Token CreateAccessToken(int second, AppUser appUser)
    {
        var securityKey = _configuration["Security:Token:SecurityKey"];
        if (string.IsNullOrEmpty(securityKey))
            throw new InvalidOperationException("Security key is not configured");

        var issuer = _configuration["Security:Token:Issuer"];
        var audience = _configuration["Security:Token:Audience"];

        Application.Dtos.Token.Token token = new();
        
        SymmetricSecurityKey symmetricSecurityKey = new(System.Text.Encoding.UTF8.GetBytes(securityKey));
        
        SigningCredentials signingCredentials = new(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        
        token.Expiration = DateTime.UtcNow.AddSeconds(second);
        JwtSecurityToken securityToken = new(
            issuer: issuer,
            audience: audience,
            expires: token.Expiration,
            notBefore: DateTime.UtcNow,
            signingCredentials: signingCredentials,
            claims: new List<Claim>
            {
                new(ClaimTypes.Name, appUser.UserName)
            }
        );
        
        JwtSecurityTokenHandler tokenHandler = new();
        token.AccessToken = tokenHandler.WriteToken(securityToken);
        token.RefreshToken = CreateRefreshToken();
        
        return token;
    }

    public Application.Dtos.Token.Token CreateAccessToken(AppUser user)
    {
        var accessTokenLifetime = _configuration.GetValue<int>("Security:JwtSettings:AccessTokenLifetimeMinutes", 120);
        return CreateAccessToken(accessTokenLifetime * 60, user); // Convert minutes to seconds
    }

    public string CreateRefreshToken()
    {
        byte[] number = new byte[32];
        using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(number);
        return Convert.ToBase64String(number);
    }
}