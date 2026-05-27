using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Ward)
            .HasMaxLength(100);

        builder.Property(x => x.District)
            .HasMaxLength(100);

        builder.Property(x => x.Province)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AddressType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(ua => ua.User)
            .WithMany(u => u.UserAddresses)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(x => x.UserId);
    }
}
