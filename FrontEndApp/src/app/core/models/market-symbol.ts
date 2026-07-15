// Display order for the header strip and market lists; anything unlisted sorts after, alphabetically.
export const PREFERRED_SYMBOL_ORDER = ['BTC', 'ETH', 'SOL', 'BNB', 'ADA', 'XRP'];

const KNOWN_QUOTES = ['USDT', 'USDC', 'BTC', 'ETH'];

/** Splits an exchange symbol like "BTCUSDT" into a display pair "BTC/USDT". */
export function symbolToPair(symbol: string): string {
  const quote = KNOWN_QUOTES.find(q => symbol.endsWith(q) && symbol.length > q.length);
  if (!quote) return symbol;
  return `${symbol.slice(0, -quote.length)}/${quote}`;
}

/** Base asset from an exchange symbol, e.g. "BTCUSDT" -> "BTC". */
export function symbolToBaseAsset(symbol: string): string {
  return symbolToPair(symbol).split('/')[0];
}
