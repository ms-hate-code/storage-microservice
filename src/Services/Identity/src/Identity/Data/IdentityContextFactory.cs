using BuildingBlocks.EFCore;
using BuildingBlocks.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.Data
{
    public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .Build();

            var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));

            var optionBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionBuilder.UseNpgsql(postgresOptions.ConnectionString)
                .UseSnakeCaseNamingConvention(); ;

            return new IdentityContext(optionBuilder.Options);
        }
    }
}
