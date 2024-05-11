using Application.Dtos.Token;
using Domain.Identity;

namespace Application.Tokens;

public interface ITokenHandler
{
    Token CreateAccessToken(AppUser user);
    string CreateRefreshToken();
}