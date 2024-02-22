using Microsoft.AspNetCore.Identity;
using Filebin.Common.Util;
using Filebin.AuthServer.Services;
using Filebin.Common.Data.Roles;
using Filebin.Domain.Auth.Abstraction.Services;

namespace Filebin.AuthServer;

public static class ConfigureServices {

    public static void AddServices(this IServiceCollection services) {
        services.AddScoped<IConfirmationMailService, MailService>();
        services.AddScoped<IPasswordResetMailService, MailService>();
        services.AddScoped<ITokenService, TokenService>();
    }

    public static async Task<IServiceProvider> ConfigureRolesAsync(this IServiceProvider services) {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var config = services.GetRequiredService<IConfiguration>();

        var roleExist = await roleManager.RoleExistsAsync(AdminRole.RoleName);

        if (!roleExist) {
            var result = await roleManager.CreateAsync(new AdminRole());
        }

        var adminUser = await userManager.FindByEmailAsync(AdminRole.AdminEmail);

        if (adminUser is null) {
            adminUser = new IdentityUser {
                UserName = AdminRole.AdminUsername,
                Email = AdminRole.AdminEmail,
            };
            var adminPassword = config.GetOrThrow("AdminDefaultPassword");

            var createAdminUser = await userManager.CreateAsync(adminUser, adminPassword);

            if (createAdminUser.Succeeded) {
                await userManager.AddToRoleAsync(adminUser, AdminRole.RoleName);
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
            }
        }
        return services;
    }
}
