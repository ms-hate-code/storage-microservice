using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metadata.Data.Configuration;

public class FileFolderConfiguration: IEntityTypeConfiguration<FileFolder.Models.FileFolder>
{
    public void Configure(EntityTypeBuilder<FileFolder.Models.FileFolder> builder)
    {
        builder.ToTable(nameof(FileFolder.Models.FileFolder));
    }
}