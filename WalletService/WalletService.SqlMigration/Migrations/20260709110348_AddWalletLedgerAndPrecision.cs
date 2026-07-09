using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletService.SqlMigration.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletLedgerAndPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "LockedAmount",
                table: "UserWalletAssets",
                type: "decimal(28,12)",
                precision: 28,
                scale: 12,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "UserWalletAssets",
                type: "decimal(28,12)",
                precision: 28,
                scale: 12,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserWalletId = table.Column<long>(type: "bigint", nullable: false),
                    AssetId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ReferenceId",
                table: "WalletTransactions",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_UserWalletId",
                table: "WalletTransactions",
                column: "UserWalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.AlterColumn<decimal>(
                name: "LockedAmount",
                table: "UserWalletAssets",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,12)",
                oldPrecision: 28,
                oldScale: 12);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "UserWalletAssets",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,12)",
                oldPrecision: 28,
                oldScale: 12);
        }
    }
}
