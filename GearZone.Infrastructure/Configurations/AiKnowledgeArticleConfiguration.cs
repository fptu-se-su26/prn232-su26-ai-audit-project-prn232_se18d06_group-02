using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations;

public sealed class AiKnowledgeArticleConfiguration : IEntityTypeConfiguration<AiKnowledgeArticle>
{
    public void Configure(EntityTypeBuilder<AiKnowledgeArticle> builder)
    {
        builder.ToTable("AiKnowledgeArticles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Keywords).HasMaxLength(1000);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(x => x.CreatedByUserId).HasMaxLength(450);
        builder.Property(x => x.UpdatedByUserId).HasMaxLength(450);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.Status, x.Category });
    }
}
