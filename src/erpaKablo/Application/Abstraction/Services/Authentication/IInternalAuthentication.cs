using Application.Dtos.Token;

namespace Application.Abstraction.Services.Authentication;

public interface IInternalAuthentication
{
    Task<Token> LoginAsync(string email, string password, int accessTokenLifetime);
    Task<Token> RefreshTokenLoginAsync(string refreshToken);
}