using Microsoft.AspNetCore.Identity;

namespace MediQueue.Models
{
    public static class DbInitializer
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Doctor", "Patient" };

            foreach (var role in roles)
            {
                bool exists = await roleManager.RoleExistsAsync(role);

                if (!exists)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        public static async Task SeedAdmin(UserManager<User> userManager)
        {
            var adminEmail = "mohamedabeid@gmail.com";

            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "MohamedAbeid",
                    PhoneNumber = "01000000000"

                };

                await userManager.CreateAsync(admin, "MohamedAbeid");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
