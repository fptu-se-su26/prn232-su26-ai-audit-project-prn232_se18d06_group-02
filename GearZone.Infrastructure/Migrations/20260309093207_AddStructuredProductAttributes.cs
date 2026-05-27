using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredProductAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComparable",
                table: "CategoryAttributes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "CategoryAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "CategoryAttributes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueType",
                table: "CategoryAttributes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductAttributeValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryAttributeId = table.Column<int>(type: "int", nullable: false),
                    CategoryAttributeOptionId = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_CategoryAttributeOptions_CategoryAttributeOptionId",
                        column: x => x.CategoryAttributeOptionId,
                        principalTable: "CategoryAttributeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_CategoryAttributes_CategoryAttributeId",
                        column: x => x.CategoryAttributeId,
                        principalTable: "CategoryAttributes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_CategoryAttributeId",
                table: "ProductAttributeValues",
                column: "CategoryAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_CategoryAttributeOptionId",
                table: "ProductAttributeValues",
                column: "CategoryAttributeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_ProductId_CategoryAttributeId",
                table: "ProductAttributeValues",
                columns: new[] { "ProductId", "CategoryAttributeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAttributeValues");

            migrationBuilder.DropColumn(
                name: "IsComparable",
                table: "CategoryAttributes");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "CategoryAttributes");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "CategoryAttributes");

            migrationBuilder.DropColumn(
                name: "ValueType",
                table: "CategoryAttributes");
        }
    }
}
