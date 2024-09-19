using BuildingBlocks.PersistMessageStore.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.PersistMessageStore.Data.Configurations
{
    public class PersistMessageConfiguration : IEntityTypeConfiguration<PersistMessage>
    {
        public void Configure(EntityTypeBuilder<PersistMessage> builder)
        {
            builder.ToTable(nameof(PersistMessage));

            builder.HasKey(x => x.Id);

            builder.Property(r => r.Id)
                .IsRequired().ValueGeneratedNever();

            builder.Property(x => x.MessageDeliveryType)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (MessageDeliveryType)Enum.Parse(typeof(MessageDeliveryType), v));

            builder.Property(x => x.MessageStatus)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (MessageStatus)Enum.Parse(typeof(MessageStatus), v));
        }
    }
}
