using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("Stores");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StoreName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.LogoUrl).HasMaxLength(1000);
            
            builder.Property(x => x.BankName).HasMaxLength(50);
            builder.Property(x => x.BankAccountName).HasMaxLength(100);
            builder.Property(x => x.BankAccountNumber).HasMaxLength(50);
            builder.Property(x => x.BankBin).HasMaxLength(20);
            
            builder.Property(x => x.TaxCode).HasMaxLength(50);
            builder.Property(x => x.Phone).HasMaxLength(50);
            builder.Property(x => x.Email).HasMaxLength(256);
            builder.Property(x => x.AddressLine).HasMaxLength(500);
            builder.Property(x => x.Province).HasMaxLength(100);
            builder.Property(x => x.IdentityCardFrontImageUrl).HasMaxLength(1000);
            builder.Property(x => x.IdentityCardBackImageUrl).HasMaxLength(1000);
            builder.Property(x => x.BusinessType).HasMaxLength(20).HasConversion<string>();

            builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasConversion<string>();
            builder.Property(x => x.RejectReason).HasMaxLength(500);
            builder.Property(x => x.LockReason).HasMaxLength(500);

            builder.Property(x => x.Latitude).HasColumnType("float");
            builder.Property(x => x.Longitude).HasColumnType("float");

            builder.HasIndex(x => x.Slug).IsUnique();
            builder.HasIndex(x => x.OwnerUserId);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.OwnerUser)
                   .WithMany(x => x.OwnedStores)
                   .HasForeignKey(x => x.OwnerUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Staffs)
                   .WithMany(x => x.StaffStores)
                   .UsingEntity(j => j.ToTable("StoreStaffs"));
        }
    }
}
