using System.Collections.Generic;

namespace WalletService.Application.Dto
{
    public class WalletOverviewDto
    {
        public decimal TotalBalanceUSDT { get; set; }
        public decimal TotalChange24h { get; set; }
        public List<WalletBalanceDto> Assets { get; set; } = new();
    }

    public class WalletBalanceDto
    {
        public required string Id { get; set; }
        public required string Symbol { get; set; }
        public required string Name { get; set; }
        public required string Network { get; set; }
        public decimal Balance { get; set; }
        public decimal LockedBalance { get; set; }
        public decimal BalanceUSDT { get; set; }
        public string DepositAddress { get; set; } = string.Empty;
        public decimal Change24h { get; set; }
        public decimal Price { get; set; }
        public bool IsDemo { get; set; }
    }
}
