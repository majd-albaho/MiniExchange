using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockchainScanner.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockChainStates",
                columns: table => new
                {
                    Network = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastProcessedBlock = table.Column<decimal>(type: "decimal(38,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockChainStates", x => x.Network);
                });

            migrationBuilder.CreateTable(
                name: "BlockchainTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Asset = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    From = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    To = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    BlockNumber = table.Column<decimal>(type: "decimal(38,0)", nullable: false),
                    GasUsed = table.Column<decimal>(type: "decimal(38,0)", nullable: false),
                    EffectiveGasPrice = table.Column<decimal>(type: "decimal(38,0)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(38,18)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredWallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredWallets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainTransactions_TransactionDateTime",
                table: "BlockchainTransactions",
                column: "TransactionDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainTransactions_TransactionHash",
                table: "BlockchainTransactions",
                column: "TransactionHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainTransactions_TransactionType",
                table: "BlockchainTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredWallets_UserId",
                table: "RegisteredWallets",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockChainStates");

            migrationBuilder.DropTable(
                name: "BlockchainTransactions");

            migrationBuilder.DropTable(
                name: "RegisteredWallets");
        }
    }
}
