using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Application.Tokens;
using Domain.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Token;

public class TokenHandler: ITokenHandler
{
    readonly IConfiguration _configuration;

    public TokenHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Application.Dtos.Token.Token CreateAccessToken(int second, AppUser appUser)
    {
        Application.Dtos.Token.Token token = new ();
        
        SymmetricSecurityKey securityKey = new(System.Text.Encoding.UTF8.GetBytes(_configuration["Token:SecurityKey"]));
        
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);
        
        token.Expiration = DateTime.UtcNow.AddSeconds(second);
        JwtSecurityToken securityToken = new(
            issuer: _configuration["Token:Issuer"],
            audience: _configuration["Token:Audience"],
            expires: token.Expiration,
            notBefore: DateTime.UtcNow,
            signingCredentials: signingCredentials,
            claims: new List<Claim>()
            {
                new(ClaimTypes.Name, appUser.UserName),
                
            }
        );
        
        
        JwtSecurityTokenHandler tokenHandler = new();
        token.AccessToken = tokenHandler.WriteToken(securityToken);

        token.RefreshToken = CreateRefreshToken();
        return token;

    }

    public Application.Dtos.Token.Token CreateAccessToken(AppUser user)
    {
        throw new NotImplementedException();
    }

    public string CreateRefreshToken()
    {
        byte[] number = new byte[32];
        using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(number);
        return Convert.ToBase64String(number);
    }
}