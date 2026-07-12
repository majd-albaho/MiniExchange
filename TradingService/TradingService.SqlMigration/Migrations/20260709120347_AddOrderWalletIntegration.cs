using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingService.SqlMigration.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderWalletIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BaseAssetId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LockedAmount",
                table: "Orders",
                type: "decimal(28,12)",
                precision: 28,
                scale: 12,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "QuoteAssetId",
                table: "Orders",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseAssetId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LockedAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "QuoteAssetId",
                table: "Orders");
        }
    }
}
