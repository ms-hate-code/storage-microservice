using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metadata.Data.Configuration;

public class MetadataConfiguration : IEntityTypeConfiguration<Metadata.Models.Metadata>
{
    public void Configure(EntityTypeBuilder<Metadata.Models.Metadata> builder)
    {
        builder.ToTable(nameof(Metadata.Models.Metadata));
    }
}