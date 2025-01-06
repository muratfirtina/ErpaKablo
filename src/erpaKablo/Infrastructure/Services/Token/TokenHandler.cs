using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Application.Tokens;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Application.Abstraction.Services.Configurations;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Token;

public class TokenHandler : ITokenHandler
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<TokenHandler> _logger;

    // Cache'lenmiş güvenlik anahtarı için özel field
    private SymmetricSecurityKey _cachedSecurityKey;
    private DateTime _securityKeyLastRefresh = DateTime.MinValue;
    private const int KeyRefreshIntervalMinutes = 60;

    public TokenHandler(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IKeyVaultService keyVaultService,
        ILogger<TokenHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task<SymmetricSecurityKey> GetSecurityKeyAsync()
    {
        // Cache'lenmiş anahtarı kontrol et
        if (_cachedSecurityKey != null && 
            DateTime.UtcNow.Subtract(_securityKeyLastRefresh).TotalMinutes < KeyRefreshIntervalMinutes)
        {
            return _cachedSecurityKey;
        }

        try
        {
            // Key Vault'tan JWT anahtarını al
            var jwtKey = await _keyVaultService.GetSecretAsync("JwtSecurityKey");
            
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogWarning("JWT key not found in Key Vault, falling back to environment variable");
                // Yedek olarak environment variable'dan al
                jwtKey = Environment.GetEnvironmentVariable("JWT_SECURITY_KEY");
            }

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT security key is not configured in either Key Vault or environment variables");
            }

            var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtKey);
            if (keyBytes.Length * 8 < 256)
            {
                throw new InvalidOperationException(
                    $"Security key must be at least 32 characters long. Current length: {keyBytes.Length * 8} bits");
            }

            // Cache'i güncelle
            _cachedSecurityKey = new SymmetricSecurityKey(keyBytes);
            _securityKeyLastRefresh = DateTime.UtcNow;

            return _cachedSecurityKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving JWT key");
            throw;
        }
    }

    public async Task<Application.Dtos.Token.Token> CreateAccessTokenAsync(int second, AppUser appUser)
    {
        var token = new Application.Dtos.Token.Token();

        var securityKey = await GetSecurityKeyAsync();
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Get user roles asynchronously
        var userRoles = await _userManager.GetRolesAsync(appUser);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, appUser.Id),
            new(ClaimTypes.Name, appUser.UserName),
            new("NameSurname", appUser.NameSurname)
        };

        // Add role claims
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Get issuer and audience from Key Vault
        var issuer = await _keyVaultService.GetSecretAsync("JwtIssuer") 
            ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new InvalidOperationException("JWT issuer is not configured");
            
        var audience = await _keyVaultService.GetSecretAsync("JwtAudience")
            ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? throw new InvalidOperationException("JWT audience is not configured");

        token.Expiration = DateTime.UtcNow.AddSeconds(second);
        var securityToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: token.Expiration,
            notBefore: DateTime.UtcNow,
            signingCredentials: signingCredentials,
            claims: claims
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        token.AccessToken = tokenHandler.WriteToken(securityToken);
        token.RefreshToken = CreateRefreshToken();

        return token;
    }

    public Task<Application.Dtos.Token.Token> CreateAccessTokenAsync(AppUser user)
    {
        // Varsayılan token süresi 120 dakika
        return CreateAccessTokenAsync(120 * 60, user);
    }

    public string CreateRefreshToken()
    {
        byte[] number = new byte[32];
        using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(number);
        return Convert.ToBase64String(number);
    }
}