using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class StoreFollowConfiguration : IEntityTypeConfiguration<StoreFollow>
    {
        public void Configure(EntityTypeBuilder<StoreFollow> builder)
        {
            builder.ToTable("StoreFollows");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.UserId, x.StoreId }).IsUnique();
            builder.HasIndex(x => x.StoreId);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.StoreFollows)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Store)
                   .WithMany(x => x.StoreFollows)
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
