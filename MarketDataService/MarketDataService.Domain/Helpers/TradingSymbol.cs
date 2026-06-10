namespace MarketDataService.Domain.Helpers
{
    public static class TradingSymbol
    {
        public static bool TryNormalize(string? symbol, out string normalizedSymbol)
        {
            normalizedSymbol = string.Empty;

            if (string.IsNullOrWhiteSpace(symbol))
            {
                return false;
            }

            var trimmedSymbol = symbol.Trim();

            if (trimmedSymbol.Length > 32)
            {
                return false;
            }

            foreach (var character in trimmedSymbol)
            {
                if (!IsAsciiLetterOrDigit(character))
                {
                    return false;
                }
            }

            normalizedSymbol = trimmedSymbol.ToUpperInvariant();
            return true;
        }

        private static bool IsAsciiLetterOrDigit(char character)
        {
            return character is >= 'A' and <= 'Z'
                or >= 'a' and <= 'z'
                or >= '0' and <= '9';
        }
    }
}
