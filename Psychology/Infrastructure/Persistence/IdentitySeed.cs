using Microsoft.AspNetCore.Identity;
using Psychology.Domain.Entities;

namespace Psychology.Infrastructure.Persistence
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();

            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var email = config["AdminSeed:Email"];
            var password = config["AdminSeed:Password"];

            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure roles exist
            foreach (var r in new[] { "Admin", "Therapist", "Client" })
            {
                if (!await roles.RoleExistsAsync(r))
                    await roles.CreateAsync(new IdentityRole(r));
            }

            // Seed admin
            var admin = await users.FindByEmailAsync(email);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Site Admin"
                };

                var result = await users.CreateAsync(admin, password);
                if (result.Succeeded)
                    await users.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
