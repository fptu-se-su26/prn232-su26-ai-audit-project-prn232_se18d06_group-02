using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePayoutTransactionCodeToStringV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionCode",
                table: "PayoutTransactions");

            migrationBuilder.AddColumn<string>(
                name: "TransactionCode",
                table: "PayoutTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionCode",
                table: "PayoutTransactions");

            migrationBuilder.AddColumn<long>(
                name: "TransactionCode",
                table: "PayoutTransactions",
                type: "bigint",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");
        }
    }
}
