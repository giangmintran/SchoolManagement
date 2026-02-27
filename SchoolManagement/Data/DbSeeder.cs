using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            using var scope = service.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var configuration = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

            // ===============================
            // 1. Seed Roles
            // ===============================

            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Create role {role} failed: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            // ===============================
            // 2. Seed Admin User
            // ===============================

            var adminEmail = configuration["SeedAdmin:Email"];
            var adminPassword = configuration["SeedAdmin:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                throw new Exception("SeedAdmin configuration missing.");

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createUserResult = await userManager
                    .CreateAsync(adminUser, adminPassword);

                if (!createUserResult.Succeeded)
                {
                    throw new Exception(
                        $"Create admin failed: " +
                        string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                }
            }

            // ===============================
            // 3. Add Admin Role
            // ===============================

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        $"Add role Admin failed: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}