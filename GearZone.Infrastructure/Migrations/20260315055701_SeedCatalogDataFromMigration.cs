using GearZone.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogDataFromMigration : Migration
    {
        private const string DemoOwnerId = "7f7dc0b4-cf3a-4eaf-b727-779b2eb1d001";
        private const string DemoOwnerEmail = "demo.store@gearzone.local";
        private const string DemoOwnerPasswordHash = "AQAAAAIAAYagAAAAEMIRj7IKXWCe/u9bxUBzNCayeojQrMNLK01j09B3++1yvKyoHDhNGfbh3ZZhmBjizA==";
        private const string DemoStoreOwnerRoleId = "4ac078a9-e2cb-4400-b30d-2082b762e81c";
        private const string DemoStoreId = "E1C9E713-5C79-4EB4-A1CB-5318908AF84D";
        private const string DemoStoreSlug = "gearzone-official";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [NormalizedName] = N'STORE OWNER')
                BEGIN
                    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                    VALUES (N'{DemoStoreOwnerRoleId}', N'Store Owner', N'STORE OWNER', N'{DemoStoreOwnerRoleId}');
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [NormalizedEmail] = N'{DemoOwnerEmail.ToUpperInvariant()}')
                BEGIN
                    INSERT INTO [AspNetUsers]
                    (
                        [Id], [FullName], [AvatarUrl], [IdentityNumber], [IdentityIssuedDate], [IdentityIssuedPlace],
                        [CreatedAt], [IsActive], [IsDeleted], [DeletedAt], [DeletedBy],
                        [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed],
                        [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed],
                        [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Address]
                    )
                    VALUES
                    (
                        N'{DemoOwnerId}', N'GearZone Demo Store Owner', NULL, NULL, NULL, NULL,
                        '2026-03-15 05:30:00', 1, 0, NULL, NULL,
                        N'{DemoOwnerEmail}', N'{DemoOwnerEmail.ToUpperInvariant()}', N'{DemoOwnerEmail}', N'{DemoOwnerEmail.ToUpperInvariant()}', 1,
                        N'{DemoOwnerPasswordHash}', N'8C4973CC-1E4B-4CB3-92A0-A50FC6887AE8', N'CA2BB294-6E20-4ADB-B7AE-3DD1548ABF98', N'0909000000', 0,
                        0, NULL, 0, 0, N'123 Tech Street, Ho Chi Minh City'
                    );
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = N'{DemoOwnerId}')
                   AND EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [NormalizedName] = N'STORE OWNER')
                   AND NOT EXISTS
                   (
                       SELECT 1
                       FROM [AspNetUserRoles]
                       WHERE [UserId] = N'{DemoOwnerId}'
                         AND [RoleId] IN (SELECT [Id] FROM [AspNetRoles] WHERE [NormalizedName] = N'STORE OWNER')
                   )
                BEGIN
                    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
                    SELECT N'{DemoOwnerId}', [Id]
                    FROM [AspNetRoles]
                    WHERE [NormalizedName] = N'STORE OWNER';
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [Stores] WHERE [Slug] = N'{DemoStoreSlug}')
                BEGIN
                    INSERT INTO [Stores]
                    (
                        [Id], [OwnerUserId], [StoreName], [Slug], [Description], [LogoUrl], [BusinessType], [TaxCode],
                        [Phone], [Email], [AddressLine], [Province], [IdentityCardFrontImageUrl], [IdentityCardBackImageUrl],
                        [BankAccountNumber], [BankAccountName], [BankName], [BankBin], [RegistrationStep], [Status],
                        [RejectReason], [LockReason], [CommissionRate], [CreatedAt], [ApprovedAt], [UpdatedAt]
                    )
                    VALUES
                    (
                        '{DemoStoreId}', N'{DemoOwnerId}', N'GearZone Official Store', N'{DemoStoreSlug}',
                        N'Official marketplace store with a seeded catalog for browsing, reviews, and shop chat demos.',
                        NULL, N'Individual', N'',
                        N'0123456789', N'official@gearzone.com', N'123 Tech Street', N'Ho Chi Minh City', NULL, NULL,
                        N'', N'', N'', N'', 1, N'Approved',
                        NULL, NULL, 0.00, '2026-03-14 10:21:46.2678332', NULL, NULL
                    );
                END
                """);

            migrationBuilder.Sql(
                $"""
                UPDATE [Stores]
                SET [Description] = COALESCE(NULLIF([Description], N''), N'Official marketplace store with a seeded catalog for browsing, reviews, and shop chat demos.')
                WHERE [Slug] = N'{DemoStoreSlug}';
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [Brands] WHERE [Slug] = N'nvidia')
                BEGIN
                {CatalogSeedMigrationSql.Brands}
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [CategoryAttributes] WHERE [Id] = 1)
                BEGIN
                {CatalogSeedMigrationSql.CategoryAttributes}
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [CategoryAttributeOptions] WHERE [Id] = 1)
                BEGIN
                {CatalogSeedMigrationSql.CategoryAttributeOptions}
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Slug] = N'asus-rog-strix-rtx-4090')
                BEGIN
                {CatalogSeedMigrationSql.Products}
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [ProductVariants])
                   AND EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 'F86BA249-DCF5-407E-AF20-002AB766E697')
                BEGIN
                {CatalogSeedMigrationSql.ProductVariants}
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [VariantAttributeValues])
                   AND EXISTS (SELECT 1 FROM [ProductVariants] WHERE [Id] = '448A4424-7ABA-4C41-99F5-F82D7CF3A5B6')
                BEGIN
                {CatalogSeedMigrationSql.VariantAttributeValues}
                END
                """);

            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [ProductAttributeValues])
                   AND EXISTS (SELECT 1 FROM [Products] WHERE [Id] = '63AB88FD-3A38-4596-A425-169DC3917EC7')
                BEGIN
                {CatalogSeedMigrationSql.ProductAttributeValues}
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                DELETE FROM [ProductImages]
                WHERE [ProductId] IN (SELECT [Id] FROM [Products] WHERE [StoreId] = '{DemoStoreId}');

                DELETE FROM [ProductAttributeValues]
                WHERE [ProductId] IN (SELECT [Id] FROM [Products] WHERE [StoreId] = '{DemoStoreId}');

                DELETE FROM [VariantAttributeValues]
                WHERE [VariantId] IN
                (
                    SELECT [Id]
                    FROM [ProductVariants]
                    WHERE [ProductId] IN (SELECT [Id] FROM [Products] WHERE [StoreId] = '{DemoStoreId}')
                );

                DELETE FROM [ProductVariants]
                WHERE [ProductId] IN (SELECT [Id] FROM [Products] WHERE [StoreId] = '{DemoStoreId}');

                DELETE FROM [Products]
                WHERE [StoreId] = '{DemoStoreId}';

                DELETE FROM [CategoryAttributeOptions]
                WHERE [Id] BETWEEN 1 AND 125;

                DELETE FROM [CategoryAttributes]
                WHERE [Id] BETWEEN 1 AND 54;

                DELETE FROM [Brands]
                WHERE [Id] BETWEEN 1 AND 31;

                DELETE FROM [Stores]
                WHERE [Id] = '{DemoStoreId}'
                  AND [OwnerUserId] = N'{DemoOwnerId}';

                DELETE FROM [AspNetUserRoles]
                WHERE [UserId] = N'{DemoOwnerId}';

                DELETE FROM [AspNetUsers]
                WHERE [Id] = N'{DemoOwnerId}'
                  AND [NormalizedEmail] = N'{DemoOwnerEmail.ToUpperInvariant()}';

                DELETE FROM [AspNetRoles]
                WHERE [Id] = N'{DemoStoreOwnerRoleId}'
                  AND [NormalizedName] = N'STORE OWNER'
                  AND NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [RoleId] = N'{DemoStoreOwnerRoleId}');
                """);
        }
    }
}
