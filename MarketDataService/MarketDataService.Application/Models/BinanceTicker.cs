using System.Globalization;

namespace MarketDataService.Application.Models
{
    public class BinanceTicker
    {
        public string e { get; set; } = default!;
        public long E { get; set; }
        public string s { get; set; } = default!;

        public string c { get; set; } = default!; // last price
        public string b { get; set; } = default!; // best bid
        public string a { get; set; } = default!; // best ask

        public string P { get; set; } = default!; // 24h price change percent
        public string h { get; set; } = default!; // 24h high
        public string l { get; set; } = default!; // 24h low
        public string v { get; set; } = default!; // 24h base-asset volume
        public string q { get; set; } = default!; // 24h quote-asset volume

        public string Symbol => s;
        public decimal LastPrice => decimal.Parse(c, CultureInfo.InvariantCulture);
        public decimal BidPrice => decimal.Parse(b, CultureInfo.InvariantCulture);
        public decimal AskPrice => decimal.Parse(a, CultureInfo.InvariantCulture);
        public decimal PriceChangePercent => decimal.Parse(P, CultureInfo.InvariantCulture);
        public decimal HighPrice => decimal.Parse(h, CultureInfo.InvariantCulture);
        public decimal LowPrice => decimal.Parse(l, CultureInfo.InvariantCulture);
        public decimal BaseVolume => decimal.Parse(v, CultureInfo.InvariantCulture);
        public decimal QuoteVolume => decimal.Parse(q, CultureInfo.InvariantCulture);
    }
}
