using Application.Abstraction.Helpers;
using Application.Abstraction.Services;
using Application.Dtos.Role;
using Application.Dtos.User;
using Application.Exceptions;
using Application.Repositories;
using Core.Application.Requests;
using Core.Persistence.Paging;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Security;

namespace Persistence.Services;

public class UserService: IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IEndpointRepository _endpointRepository;

    public UserService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IEndpointRepository endpointRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _endpointRepository = endpointRepository;
    }

    public async Task UpdateRefreshTokenAsync(string refreshToken, AppUser? user, DateTime accessTokenDateTime, int refreshTokenLifetime)
    {
        if (user != null)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenEndDateTime = accessTokenDateTime.AddSeconds(refreshTokenLifetime);
            await _userManager.UpdateAsync(user);
        }
        else
            throw new NotFoundUserExceptions();
    }

    public async  Task UpdateForgotPasswordAsync(string userId, string resetToken, string newPassword)
    {
        AppUser? user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            resetToken = resetToken.UrlDecode();

            IdentityResult result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (result.Succeeded)
                await _userManager.UpdateSecurityStampAsync(user);
            else
                throw new ResetPasswordException();
        }
    }

    public async Task<List<AppUser>> GetAllUsersAsync(PageRequest pageRequest)
    {
        IQueryable<AppUser> userQuery = _userManager.Users;

        if (pageRequest.PageIndex == -1 && pageRequest.PageSize == -1)
        {
            // Tüm kullanıcıları getir
            var allUsers = await userQuery.ToListAsync();

            return allUsers;
        }
        else
        {
            // Pagination yap
            IPaginate<AppUser> users = await userQuery.ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize);
            return users.Items.ToList();
        }
    }


    public async Task AssignRoleToUserAsync(string userId, List<RoleDto>? roles)
    {
        AppUser? user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRolesAsync(user, roles.Select(x => x.RoleName).ToList());
        }
    }

    public async Task<List<RoleDto>> GetRolesToUserAsync(string userIdOrName)
    {
        
        AppUser? user = await _userManager.FindByIdAsync(userIdOrName);
        if (user == null)
            user = await _userManager.FindByNameAsync(userIdOrName);
        if (user != null)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            return await _roleManager.Roles.Where(x => userRoles.Contains(x.Name)).Select(x => new RoleDto()
            {
                RoleName = x.Name,
                RoleId = x.Id
            }).ToListAsync();
        }

        return new List<RoleDto>();
    }

    public async Task<bool> HasRolePermissionToEndpointAsync(string name, string code)
    {
        var userRoles = await GetRolesToUserAsync(name);
        if (!userRoles.Any())
            return false;

        var endpoint = await _endpointRepository.GetAsync(
            predicate: e => e.Code == code,
            include: e => e.Include(x => x.Roles),
            enableTracking: false
        );

        if (endpoint == null)
            return false;

        var endpointRoles = endpoint.Roles.Select(r => r.Name).ToList();
        
        return userRoles.Any(userRole => endpointRoles.Contains(userRole.RoleName));
    }
}