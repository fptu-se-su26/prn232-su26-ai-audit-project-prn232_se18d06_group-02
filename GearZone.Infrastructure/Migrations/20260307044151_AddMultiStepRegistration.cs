using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiStepRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegistrationStep",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationStep",
                table: "Stores");
        }
    }
}
