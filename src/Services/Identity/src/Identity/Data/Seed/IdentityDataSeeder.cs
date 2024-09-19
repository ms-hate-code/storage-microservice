using BuildingBlocks.Constants;
using BuildingBlocks.EFCore;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Identity.Data.Seed
{
    public class IdentityDataSeeder(
        UserManager<AppUser> _userManager,
        RoleManager<IdentityRole> _roleManager
    ) : IDataSeeder
    {
        public async Task SeedAllAsync()
        {
            await SeedRoles();

            await SeedUsers();
        }

        private async Task SeedRoles()
        {
            if (await _roleManager.RoleExistsAsync(Common.SystemRole.ADMIN) == false)
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = Common.SystemRole.ADMIN });
            }

            if (await _roleManager.RoleExistsAsync(Common.SystemRole.USER) == false)
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = Common.SystemRole.USER });
            }
        }

        private async Task SeedUsers()
        {
            if (await _userManager.FindByEmailAsync("admin@admin.com") == null)
            {
                var result = await _userManager.CreateAsync(IdentityInitialData.Users.First(), "Admin@123456");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(IdentityInitialData.Users.First(), Common.SystemRole.ADMIN);
                }
            }

            if (await _userManager.FindByNameAsync("user@user.com") == null)
            {
                var result = await _userManager.CreateAsync(IdentityInitialData.Users.Last(), "User@123456");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(IdentityInitialData.Users.Last(), Common.SystemRole.USER);
                }
            }
        }
    }
}
