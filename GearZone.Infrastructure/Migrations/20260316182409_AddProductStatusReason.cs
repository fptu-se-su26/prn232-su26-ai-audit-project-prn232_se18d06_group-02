using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStatusReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "Products");
        }
    }
}
