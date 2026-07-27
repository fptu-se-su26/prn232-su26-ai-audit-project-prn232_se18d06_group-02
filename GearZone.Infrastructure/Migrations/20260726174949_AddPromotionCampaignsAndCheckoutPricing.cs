using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionCampaignsAndCheckoutPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VoucherUsages_VoucherId",
                table: "VoucherUsages");

            migrationBuilder.AddColumn<DateTime>(
                name: "RedeemedAt",
                table: "VoucherUsages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleasedAt",
                table: "VoucherUsages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "VoucherUsages",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Redeemed");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Vouchers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionableAmount",
                table: "SubOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionDiscountAmount",
                table: "SubOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SellerVoucherDiscountAmount",
                table: "SubOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetShippingFee",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingDiscountAmount",
                table: "Shipments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CheckoutRequestId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderVoucherCodeSnapshot",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderVoucherScopeSnapshot",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingVoucherCodeSnapshot",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingVoucherScopeSnapshot",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalUnitPriceSnapshot",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PromotionCampaignId",
                table: "OrderItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionDiscountAmount",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PromotionDiscountPerUnit",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PromotionNameSnapshot",
                table: "OrderItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            // Preserve historical financial values. Existing orders were created
            // before promotion snapshots existed, so their effective price is also
            // their original price and no historic payout is recalculated.
            migrationBuilder.Sql(
                """
                UPDATE [OrderItems]
                SET [OriginalUnitPriceSnapshot] = [UnitPriceSnapshot],
                    [PromotionDiscountPerUnit] = 0,
                    [PromotionDiscountAmount] = 0;

                UPDATE [SubOrders]
                SET [CommissionableAmount] = [Subtotal],
                    [PromotionDiscountAmount] = 0,
                    [SellerVoucherDiscountAmount] = 0;

                UPDATE [Shipments]
                SET [NetShippingFee] = [ShippingFee],
                    [ShippingDiscountAmount] = 0;

                UPDATE [VoucherUsages]
                SET [Status] = N'Redeemed',
                    [RedeemedAt] = [UsedAt],
                    [ReleasedAt] = NULL;
                """);

            migrationBuilder.CreateTable(
                name: "PromotionCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DiscountType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalQuantityLimit = table.Column<int>(type: "int", nullable: false),
                    ReservedQuantity = table.Column<int>(type: "int", nullable: false),
                    RedeemedQuantity = table.Column<int>(type: "int", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionCampaigns_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PromotionProducts",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionProducts", x => new { x.CampaignId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_PromotionProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionProducts_PromotionCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "PromotionCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionReservations_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromotionReservations_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionReservations_PromotionCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "PromotionCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsages_VoucherId_OrderId",
                table: "VoucherUsages",
                columns: new[] { "VoucherId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsages_VoucherId_UserId_Status",
                table: "VoucherUsages",
                columns: new[] { "VoucherId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_CheckoutRequestId",
                table: "Orders",
                columns: new[] { "UserId", "CheckoutRequestId" },
                unique: true,
                filter: "[CheckoutRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PromotionCampaignId",
                table: "OrderItems",
                column: "PromotionCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_StoreId_IsEnabled",
                table: "PromotionCampaigns",
                columns: new[] { "StoreId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_StoreId_StartAt_EndAt",
                table: "PromotionCampaigns",
                columns: new[] { "StoreId", "StartAt", "EndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionProducts_ProductId",
                table: "PromotionProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionReservations_CampaignId_Status",
                table: "PromotionReservations",
                columns: new[] { "CampaignId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionReservations_OrderId",
                table: "PromotionReservations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionReservations_OrderItemId",
                table: "PromotionReservations",
                column: "OrderItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_PromotionCampaigns_PromotionCampaignId",
                table: "OrderItems",
                column: "PromotionCampaignId",
                principalTable: "PromotionCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_PromotionCampaigns_PromotionCampaignId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "PromotionProducts");

            migrationBuilder.DropTable(
                name: "PromotionReservations");

            migrationBuilder.DropTable(
                name: "PromotionCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_VoucherUsages_VoucherId_OrderId",
                table: "VoucherUsages");

            migrationBuilder.DropIndex(
                name: "IX_VoucherUsages_VoucherId_UserId_Status",
                table: "VoucherUsages");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_CheckoutRequestId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_PromotionCampaignId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "RedeemedAt",
                table: "VoucherUsages");

            migrationBuilder.DropColumn(
                name: "ReleasedAt",
                table: "VoucherUsages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VoucherUsages");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "CommissionableAmount",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "PromotionDiscountAmount",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "SellerVoucherDiscountAmount",
                table: "SubOrders");

            migrationBuilder.DropColumn(
                name: "NetShippingFee",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingDiscountAmount",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "CheckoutRequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderVoucherCodeSnapshot",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderVoucherScopeSnapshot",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingVoucherCodeSnapshot",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingVoucherScopeSnapshot",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OriginalUnitPriceSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PromotionCampaignId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PromotionDiscountAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PromotionDiscountPerUnit",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PromotionNameSnapshot",
                table: "OrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsages_VoucherId",
                table: "VoucherUsages",
                column: "VoucherId");
        }
    }
}
