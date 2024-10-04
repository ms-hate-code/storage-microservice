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
    
    public DbSet<FileObject.Models.FileObject> FileObjects { get; set; }
    public DbSet<FileFolder.Models.FileFolder> FileFolders { get; set; }
}