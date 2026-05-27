using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("ProductReviews", table =>
            {
                table.HasCheckConstraint("CK_ProductReviews_Rating", "[Rating] >= 1 AND [Rating] <= 5");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BuyerUserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(x => x.Rating)
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasMaxLength(2000);

            builder.Property(x => x.SellerReplyContent)
                .HasMaxLength(2000);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.OrderItemId)
                .IsUnique();

            builder.HasIndex(x => new { x.ProductId, x.CreatedAt });
            builder.HasIndex(x => new { x.StoreId, x.CreatedAt });
            builder.HasIndex(x => new { x.BuyerUserId, x.CreatedAt });

            builder.HasOne(x => x.OrderItem)
                .WithOne(x => x.Review)
                .HasForeignKey<ProductReview>(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BuyerUser)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.BuyerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
