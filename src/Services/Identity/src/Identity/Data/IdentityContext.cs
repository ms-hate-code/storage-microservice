using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Identity.Data
{
    public class IdentityContext(
        DbContextOptions<IdentityContext> options
    )
        : IdentityDbContext<AppUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach (var type in builder.Model.GetEntityTypes())
            {
                var tableName = type.GetTableName();
                if (tableName != null && tableName.StartsWith("AspNet"))
                {
                    type.SetTableName(tableName[6..]);
                }
            }
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
