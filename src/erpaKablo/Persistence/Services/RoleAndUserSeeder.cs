using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Services;

public static class RoleAndUserSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = role,
                    NormalizedName = role.ToUpper()
                });
            }
        }
    }

    /*public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var adminUser = await userManager.FindByNameAsync("karafirtina");
        
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "karafirtina",
                Email = "murat@firtina.com",
                EmailConfirmed = true,
                NameSurname = "Murat FIRTINA"
            };

            var result = await userManager.CreateAsync(adminUser, "111");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Admin kullanıcısı oluşturulurken hata oluştu: {errors}");
            }
        }
    }*/

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await SeedRolesAsync(serviceProvider);
        //await SeedAdminUserAsync(serviceProvider);
    }
}