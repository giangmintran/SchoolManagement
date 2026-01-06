using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            using var scope = service.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<string>>>();

            // 1. Seed Roles
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<string>(role));
                }
            }

            // 2. Seed Admin User
            string adminEmail = "admin@gmail.com";
            string adminPassword = "Admin@123"; // đạt chuẩn Identity

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
				{
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ",
                        createUserResult.Errors.Select(e => e.Description));
                    throw new Exception($"Create admin failed: {errors}");
                }
            }

            // 3. Gán role Admin (chỉ khi chưa có)
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

    }
}