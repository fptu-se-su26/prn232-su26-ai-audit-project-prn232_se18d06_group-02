using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToPaymentAndPayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TransactionCode",
                table: "PayoutTransactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "PaymentCode",
                table: "Payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionCode",
                table: "PayoutTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentCode",
                table: "Payments");
        }
    }
}
