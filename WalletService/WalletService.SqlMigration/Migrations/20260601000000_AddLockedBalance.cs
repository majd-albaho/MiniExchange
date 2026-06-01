using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletService.SqlMigration.Migrations
{
    /// <inheritdoc />
    public partial class AddLockedBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LockedBalance",
                table: "UserWallets",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedBalance",
                table: "UserWallets");
        }
    }
}
