using BuildingBlocks.EFCore;
using BuildingBlocks.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Metadata.Data;

public class MetadataContextFactory: IDesignTimeDbContextFactory<MetadataContext>
{
    public MetadataContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .Build();

        var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));

        var optionBuilder = new DbContextOptionsBuilder<MetadataContext>();
        optionBuilder.UseNpgsql(postgresOptions.ConnectionString)
            .UseSnakeCaseNamingConvention(); ;

        return new MetadataContext(optionBuilder.Options);
    }
}