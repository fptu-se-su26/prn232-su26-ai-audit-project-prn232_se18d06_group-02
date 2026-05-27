using GearZone.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260315071500_ReconcileDemoSellerOwnership")]
    public partial class ReconcileDemoSellerOwnership : Migration
    {
        private const string DemoOwnerId = "7f7dc0b4-cf3a-4eaf-b727-779b2eb1d001";
        private const string DemoOwnerEmail = "demo.store@gearzone.local";
        private const string DemoOwnerPasswordHash = "AQAAAAIAAYagAAAAEMIRj7IKXWCe/u9bxUBzNCayeojQrMNLK01j09B3++1yvKyoHDhNGfbh3ZZhmBjizA==";
        private const string DemoStoreOwnerRoleId = "4ac078a9-e2cb-4400-b30d-2082b762e81c";
        private const string DemoStoreSlug = "gearzone-official";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [NormalizedName] = N'STORE OWNER')
                BEGIN
                    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                    VALUES (N'{DemoStoreOwnerRoleId}', N'Store Owner', N'STORE OWNER', N'{DemoStoreOwnerRoleId}');
                END

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

                UPDATE [Stores]
                SET [OwnerUserId] = N'{DemoOwnerId}'
                WHERE [Slug] = N'{DemoStoreSlug}'
                  AND [OwnerUserId] <> N'{DemoOwnerId}';

                DELETE FROM [ProductImages]
                WHERE [ImageUrl] LIKE N'https://placehold.co/1200x900/%';

                UPDATE [Stores]
                SET [LogoUrl] = NULL
                WHERE [Slug] = N'{DemoStoreSlug}'
                  AND [LogoUrl] LIKE N'data:image/svg+xml;%';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                DECLARE @AdminUserId nvarchar(450);
                SELECT TOP (1) @AdminUserId = [Id]
                FROM [AspNetUsers]
                WHERE [NormalizedEmail] = N'ADMIN@GMAIL.COM';

                IF @AdminUserId IS NOT NULL
                BEGIN
                    UPDATE [Stores]
                    SET [OwnerUserId] = @AdminUserId
                    WHERE [Slug] = N'{DemoStoreSlug}'
                      AND [OwnerUserId] = N'{DemoOwnerId}';
                END
                """);
        }
    }
}
