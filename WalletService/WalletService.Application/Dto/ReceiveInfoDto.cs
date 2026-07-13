namespace WalletService.Application.Dto
{
    public class ReceiveInfoDto
    {
        public required string Symbol { get; set; }
        public required string Network { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? Memo { get; set; }
        public decimal MinDeposit { get; set; }
        public int Confirmations { get; set; }

        /// <summary>Demo tokens have no on-chain deposit address; they are added via the "Add Demo Token" flow.</summary>
        public bool IsDemo { get; set; }
    }
}
