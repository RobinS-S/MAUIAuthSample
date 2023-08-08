using AuthenticationSample.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationSample.Data;

public class DatabaseSeeder
{
    public static async Task SeedDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<Config>();

        await dbContext.Database.MigrateAsync();

        var roleExist = await roleManager.RoleExistsAsync(Roles.AdminRoleName);
        if (!roleExist) await roleManager.CreateAsync(new IdentityRole(Roles.AdminRoleName));

        if (string.IsNullOrWhiteSpace(configuration.AdminUserEmail) ||
            string.IsNullOrWhiteSpace(configuration.AdminUserPassword))
            throw new Exception(
                "You need to provide a default user account which will be created with the Admin role, keys: AppSettings:AdminUserEmail and AppSettings:AdminUserPassword");


        var defaultUser = new ApplicationUser
        {
            UserName = configuration.AdminUserEmail,
            Email = configuration.AdminUserEmail
        };

        var user = await userManager.FindByEmailAsync(configuration.AdminUserEmail);

        if (user == null)
        {
            var createPowerUser = await userManager.CreateAsync(defaultUser, configuration.AdminUserPassword);
            if (createPowerUser.Succeeded) await userManager.AddToRoleAsync(defaultUser, Roles.AdminRoleName);
        }
        else
        {
            var isAdmin = await userManager.IsInRoleAsync(user, Roles.AdminRoleName);
            if (!isAdmin) await userManager.AddToRoleAsync(user, Roles.AdminRoleName);
        }
    }
}