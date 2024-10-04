using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metadata.Data.Configuration;

public class FileObjectConfiguration : IEntityTypeConfiguration<FileObject.Models.FileObject>
{
    public void Configure(EntityTypeBuilder<FileObject.Models.FileObject> builder)
    {
        builder.ToTable(nameof(FileObject.Models.FileObject));
    }
}