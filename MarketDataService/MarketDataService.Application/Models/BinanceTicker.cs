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

        public string Symbol => s;
        public decimal LastPrice => decimal.Parse(c, CultureInfo.InvariantCulture);
        public decimal BidPrice => decimal.Parse(b, CultureInfo.InvariantCulture);
        public decimal AskPrice => decimal.Parse(a, CultureInfo.InvariantCulture);
    }
}
