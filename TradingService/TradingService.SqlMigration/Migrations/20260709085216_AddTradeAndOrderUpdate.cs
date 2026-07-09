using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingService.SqlMigration.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeAndOrderUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PairSymbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BuyOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: false),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trades_BuyOrderId",
                table: "Trades",
                column: "BuyOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_PairSymbol",
                table: "Trades",
                column: "PairSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_SellOrderId",
                table: "Trades",
                column: "SellOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}
