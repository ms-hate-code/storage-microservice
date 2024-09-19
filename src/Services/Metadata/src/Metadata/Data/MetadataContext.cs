using System.Reflection;
using BuildingBlocks.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Metadata.Data;

public class MetadataContext
(
    DbContextOptions<MetadataContext> options
)
    : AppDbContextBase(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    
    public DbSet<Metadata.Models.Metadata> Metadatas { get; set; }
}