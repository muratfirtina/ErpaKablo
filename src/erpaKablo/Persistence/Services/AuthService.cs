using System.Text.Json;
using Application.Abstraction.Helpers;
using Application.Abstraction.Services;
using Application.Dtos.Token;
using Application.Exceptions;
using Application.Tokens;
using Domain.Identity;
using Google.Apis.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenHandler _tokenHandler;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IUserService _userService;
    private readonly IMailService _mailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    

    public AuthService(IConfiguration configuration, UserManager<AppUser> userManager, ITokenHandler tokenHandler, SignInManager<AppUser> signInManager, IUserService userService, IMailService mailService, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _userManager = userManager;
        _tokenHandler = tokenHandler;
        _signInManager = signInManager;
        _userService = userService;
        _mailService = mailService;
        _httpContextAccessor = httpContextAccessor;
    }

    async Task<Token>CreateUserExternalLoginAsync(AppUser? user,string email, string name, int accessTokenLifetime, UserLoginInfo info)
    {
        bool result = user != null;
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = email,
                    UserName = email,
                    NameSurname = name,
                };
                IdentityResult identityResult = await _userManager.CreateAsync(user);
                result = identityResult.Succeeded;
            }
        }

        if (result)
        {
            await _userManager.AddLoginAsync(user, info);

            Token token = _tokenHandler.CreateAccessToken(accessTokenLifetime, user);
            await _userService.UpdateRefreshTokenAsync(token.RefreshToken, user, token.Expiration,refreshTokenLifetime:5);
            return token;
        }
        throw new Exception("Invalid external authentication.");
    }

    /*public async Task<Token> FacebookLoginAsync(string authToken, int accessTokenLifetime)
    {
        string accessTokenResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/oauth/" +
                                                                      $"access_token?client_id={_configuration["ExternalLoginSettings:Facebook:Client_ID"]}" +
                                                                      $"&client_secret={_configuration["ExternalLoginSettings:Facebook:Client_Secret"]}" +
                                                                      $"&grant_type=client_credentials");
        FacebookAccessTokenResponse? facebookAccessTokenResponse 
            = JsonSerializer.Deserialize<FacebookAccessTokenResponse>(accessTokenResponse);
        
        string userAccessTokenValidation = await _httpClient.GetStringAsync($"https://graph.facebook.com/debug_token?" +
                                                                            $"input_token={authToken}" +
                                                                            $"&access_token={facebookAccessTokenResponse?.AccessToken}");
        
        FacebookUserAccessTokenValidation? validation 
            = JsonSerializer.Deserialize<FacebookUserAccessTokenValidation>(userAccessTokenValidation);
        if (validation?.Data.IsValid != null)
        {
            string userInfoResponse = await _httpClient.GetStringAsync($"https://graph.facebook.com/me?fields=email,name" +
                                                                       $"&access_token={authToken}");
            
            FacebookUserInfoResponse? facebookUserInfo
                = JsonSerializer.Deserialize<FacebookUserInfoResponse>(userInfoResponse);
            
            var info = new UserLoginInfo("FACEBOOK",validation.Data.UserId, "FACEBOOK");
            AppUser user =
                await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            return await CreateUserExternalLoginAsync(user,facebookUserInfo.Email,facebookUserInfo.Name,accessTokenLifetime,info);
        }
        throw new Exception("Invalid external authentication.");
    }*/
    

    /*public async Task<Token> GoogleLoginAsync(string idToken, int accessTokenLifetime)
    {
        var settins = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience =
                new List<string?>() { _configuration["ExternalLoginSettings:Google:Client_ID"] },
        };
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settins);
        var info = new UserLoginInfo("GOOGLE", payload.Subject, "GOOGLE");
        AppUser user =
            await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        
        return await CreateUserExternalLoginAsync(user,payload.Email,payload.Name,accessTokenLifetime,info);

    }*/


    public async Task<Token> LoginAsync(string userNameOrEmail, string password, int accessTokenLifetime)
    {
        var user = await _userManager.FindByNameAsync(userNameOrEmail);
        if (user == null)
            user = await _userManager.FindByEmailAsync(userNameOrEmail);
        if (user == null)
            throw new NotFoundUserExceptions();

        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
        if (result.Succeeded) // Authentication başarılı
        {
            Token token = _tokenHandler.CreateAccessToken(accessTokenLifetime, user);
            await _userService.UpdateRefreshTokenAsync(token.RefreshToken, user, token.Expiration, refreshTokenLifetime: 12000);

            // HTTPOnly Cookie olarak token set ediliyor.
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // JavaScript'ten erişilmez
                Secure = true,   // HTTPS üzerinde çalışır
                Expires = token.Expiration
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", token.AccessToken, cookieOptions);
        
            return token;
        }
    
        throw new AuthenticationErrorException();
    }

    public async Task<AppUser?> LogoutAsync()
    {
        
        AppUser? user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenEndDateTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
        return null;
        
    }

    public async Task<Token> RefreshTokenLoginAsync(string refreshToken)
    {
        AppUser? user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user != null && user?.RefreshTokenEndDateTime > DateTime.UtcNow)
        {
            Token token = _tokenHandler.CreateAccessToken(8000, user);
            await _userService.UpdateRefreshTokenAsync(token.RefreshToken, user, token.Expiration,refreshTokenLifetime:12000);
            return token;
        }
        else
            throw new AuthenticationErrorException();

    }

    public async Task PasswordResetAsync(string email)
    {
        AppUser? user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            resetToken = resetToken.UrlEncode();

            await _mailService.SendPasswordResetEmailAsync(user.Email, user.Id, resetToken);
        }
    }

    public async Task<bool> VerifyResetPasswordTokenAsync(string userId, string resetToken)
    {
        AppUser? user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            resetToken = resetToken.UrlDecode();
             
            return await _userManager.VerifyUserTokenAsync(
                user,_userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", resetToken);
                
        }
        return false;
    }
}