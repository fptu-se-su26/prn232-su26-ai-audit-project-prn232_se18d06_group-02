using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace GearZone.Infrastructure.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            var superAdminEmail = configuration["ADMIN_ACCOUNT_EMAIL"];
            var superAdminPassword = configuration["ADMIN_ACCOUNT_PASSWORD"];

            if (string.IsNullOrWhiteSpace(superAdminEmail) ||
                string.IsNullOrWhiteSpace(superAdminPassword))
            {
                throw new Exception("AdminAccount environment variables (ADMIN_ACCOUNT_EMAIL, ADMIN_ACCOUNT_PASSWORD) are missing.");
            }

            string[] roles = new[]
            {
            "Super Admin",
            "Staff",
            "Store Owner",
            "Customer"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Cannot create role {role}");
                    }
                }
            }

            var existingUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (existingUser != null)
            {
                if (!await userManager.IsInRoleAsync(existingUser, "Super Admin"))
                {
                    await userManager.AddToRoleAsync(existingUser, "Super Admin");
                }
                return;
            }

            var user = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, superAdminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Cannot create SuperAdmin: {errors}");
            }

            await userManager.AddToRoleAsync(user, "Super Admin");
        }
    }
}
