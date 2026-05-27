using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalCartAndUserAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Stores_StoreId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_StoreId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Carts");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Carts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_StoreId",
                table: "Carts",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Stores_StoreId",
                table: "Carts",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id");
        }
    }
}
