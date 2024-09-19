using Identity.Identity.Models;

namespace Identity.Data.Seed
{
    public class IdentityInitialData
    {
        public static List<AppUser> Users { get; }

        static IdentityInitialData()
        {
            Users =
            [
                new() {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Admin",
                    LastName = "Admin",
                    UserName = "admin",
                    Email = "admin@admin.com",
                    SecurityStamp = Guid.NewGuid().ToString()
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "User",
                    LastName = "User",
                    UserName = "user",
                    Email = "user@user.com",
                    SecurityStamp = Guid.NewGuid().ToString()
                }
            ];
        }
    }
}