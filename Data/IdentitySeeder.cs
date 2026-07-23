using Microsoft.AspNetCore.Identity;

namespace StudentRegistrationWebApp.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var roleName in new[] { "Administrator", "Student" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Could not create the {roleName} role.");
                }
            }
        }

        const string adminEmail = "admin@app.com";
        var administrator = await userManager.FindByEmailAsync(adminEmail);
        if (administrator is null)
        {
            administrator = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(administrator, "Admin@123!");
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Could not create the administrator account.");
            }
        }

        if (!await userManager.IsInRoleAsync(administrator, "Administrator"))
        {
            var roleResult = await userManager.AddToRoleAsync(administrator, "Administrator");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Could not assign the Administrator role.");
            }
        }
    }
}
