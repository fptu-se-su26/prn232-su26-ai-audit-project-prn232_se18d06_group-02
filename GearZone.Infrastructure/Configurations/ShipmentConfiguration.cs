using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("Shipments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ShippingFee).HasColumnType("decimal(18,2)");
            builder.Property(x => x.DistanceKm).HasColumnType("float");
            builder.Property(x => x.TrackingNumber).HasMaxLength(100);
            builder.Property(x => x.ShippingProvider).HasMaxLength(100);

            builder.HasOne(x => x.Order)
                   .WithMany(o => o.Shipments)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Store)
                   .WithMany()
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
