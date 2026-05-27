using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GearZone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayoutBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalStores = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    HoldReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovedByAdminId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayoutTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayoutBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankAccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderCount = table.Column<int>(type: "int", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PayOSTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExcludeReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutTransactions_PayoutBatches_PayoutBatchId",
                        column: x => x.PayoutBatchId,
                        principalTable: "PayoutBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayoutTransactions_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayoutItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayoutTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsExcluded = table.Column<bool>(type: "bit", nullable: false),
                    ExcludeReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayoutItems_PayoutTransactions_PayoutTransactionId",
                        column: x => x.PayoutTransactionId,
                        principalTable: "PayoutTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutItems_OrderId",
                table: "PayoutItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutItems_PayoutTransactionId",
                table: "PayoutItems",
                column: "PayoutTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutTransactions_PayoutBatchId",
                table: "PayoutTransactions",
                column: "PayoutBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutTransactions_StoreId",
                table: "PayoutTransactions",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayoutItems");

            migrationBuilder.DropTable(
                name: "PayoutTransactions");

            migrationBuilder.DropTable(
                name: "PayoutBatches");
        }
    }
}
