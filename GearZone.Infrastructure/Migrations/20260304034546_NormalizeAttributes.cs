using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VariantAttributeValues_CategoryAttributes_CategoryAttributeId",
                table: "VariantAttributeValues");

            migrationBuilder.DropIndex(
                name: "IX_VariantAttributeValues_CategoryAttributeId_Value",
                table: "VariantAttributeValues");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "VariantAttributeValues");

            migrationBuilder.AddColumn<int>(
                name: "CategoryAttributeOptionId",
                table: "VariantAttributeValues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CategoryAttributeOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryAttributeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryAttributeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryAttributeOptions_CategoryAttributes_CategoryAttributeId",
                        column: x => x.CategoryAttributeId,
                        principalTable: "CategoryAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributeValues_CategoryAttributeId_CategoryAttributeOptionId",
                table: "VariantAttributeValues",
                columns: new[] { "CategoryAttributeId", "CategoryAttributeOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributeValues_CategoryAttributeOptionId",
                table: "VariantAttributeValues",
                column: "CategoryAttributeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAttributeOptions_CategoryAttributeId",
                table: "CategoryAttributeOptions",
                column: "CategoryAttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_VariantAttributeValues_CategoryAttributeOptions_CategoryAttributeOptionId",
                table: "VariantAttributeValues",
                column: "CategoryAttributeOptionId",
                principalTable: "CategoryAttributeOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VariantAttributeValues_CategoryAttributes_CategoryAttributeId",
                table: "VariantAttributeValues",
                column: "CategoryAttributeId",
                principalTable: "CategoryAttributes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VariantAttributeValues_CategoryAttributeOptions_CategoryAttributeOptionId",
                table: "VariantAttributeValues");

            migrationBuilder.DropForeignKey(
                name: "FK_VariantAttributeValues_CategoryAttributes_CategoryAttributeId",
                table: "VariantAttributeValues");

            migrationBuilder.DropTable(
                name: "CategoryAttributeOptions");

            migrationBuilder.DropIndex(
                name: "IX_VariantAttributeValues_CategoryAttributeId_CategoryAttributeOptionId",
                table: "VariantAttributeValues");

            migrationBuilder.DropIndex(
                name: "IX_VariantAttributeValues_CategoryAttributeOptionId",
                table: "VariantAttributeValues");

            migrationBuilder.DropColumn(
                name: "CategoryAttributeOptionId",
                table: "VariantAttributeValues");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "VariantAttributeValues",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributeValues_CategoryAttributeId_Value",
                table: "VariantAttributeValues",
                columns: new[] { "CategoryAttributeId", "Value" });

            migrationBuilder.AddForeignKey(
                name: "FK_VariantAttributeValues_CategoryAttributes_CategoryAttributeId",
                table: "VariantAttributeValues",
                column: "CategoryAttributeId",
                principalTable: "CategoryAttributes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
