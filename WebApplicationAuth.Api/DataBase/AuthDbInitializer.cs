using Microsoft.AspNetCore.Identity;
using WebApplicationAuth.Api.DataBase.Helpers;

namespace WebApplicationAuth.Api.DataBase
{
    public class AuthDbInitializer
    {
        public static async Task SeedRolesToDbAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // The ConcurrencyStamp will be NULL in DB in .NET8, it will default to Guid.NewGuid().ToString(); in .NET 10.

                if (!await roleManager.RoleExistsAsync(UserRoles.Manager))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));

                if (!await roleManager.RoleExistsAsync(UserRoles.Student))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Student));

                if (!await roleManager.RoleExistsAsync(UserRoles.Administrator))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Administrator));
            }
        }
    }
}
