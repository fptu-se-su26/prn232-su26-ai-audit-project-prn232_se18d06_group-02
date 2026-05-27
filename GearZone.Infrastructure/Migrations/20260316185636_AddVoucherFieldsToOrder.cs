using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherFieldsToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_AspNetUsers_ChangedByUserId",
                table: "OrderStatusHistories");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedByUserId",
                table: "OrderStatusHistories",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<decimal>(
                name: "OrderDiscountAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderVoucherId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingDiscountAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingVoucherId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderVoucherId",
                table: "Orders",
                column: "OrderVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingVoucherId",
                table: "Orders",
                column: "ShippingVoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Vouchers_OrderVoucherId",
                table: "Orders",
                column: "OrderVoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Vouchers_ShippingVoucherId",
                table: "Orders",
                column: "ShippingVoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_AspNetUsers_ChangedByUserId",
                table: "OrderStatusHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Vouchers_OrderVoucherId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Vouchers_ShippingVoucherId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_AspNetUsers_ChangedByUserId",
                table: "OrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderVoucherId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingVoucherId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderVoucherId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingVoucherId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ChangedByUserId",
                table: "OrderStatusHistories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_AspNetUsers_ChangedByUserId",
                table: "OrderStatusHistories",
                column: "ChangedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
