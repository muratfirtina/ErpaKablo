using Application.Dtos.Role;
using Application.Dtos.User;
using Core.Application.Requests;
using Core.Persistence.Paging;
using Domain.Identity;

namespace Application.Abstraction.Services;

public interface IUserService
{
    //Task<CreateUserResponse> CreateAsync(CreateUserDto model);
    Task UpdateRefreshTokenAsync(string refreshToken, AppUser user, DateTime accessTokenDateTime, int refreshTokenLifetime);
    Task UpdateForgotPasswordAsync(string userId, string resetToken, string newPassword);
    Task<List<AppUser>>  GetAllUsersAsync(PageRequest pageRequest);
   // Task<UserDto> GetUserByIdAsync(string id);
   // Task<UserDto> GetUserByNameAsync(string name);
    //Task DeleteUserAsync(string id);
    Task AssignRoleToUserAsync(string userId, List<RoleDto> roles);
    public Task<List<RoleDto>> GetRolesToUserAsync(string userIdOrName);
    Task<bool>HasRolePermissionToEndpointAsync(string name, string code);
}