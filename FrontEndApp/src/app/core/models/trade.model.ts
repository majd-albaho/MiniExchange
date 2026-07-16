export interface TradePair {
  symbol: string;
  baseAsset: string;
  quoteAsset: string;
  lastPrice: number;
  change24h: number;
  high24h: number;
  low24h: number;
  volume24h: number;
  logoUrl: string;
}

export interface OrderBookEntry {
  price: number;
  amount: number;
  total: number;
}

export interface OrderBook {
  pair: string;
  asks: OrderBookEntry[];
  bids: OrderBookEntry[];
  lastUpdateTime: string;
}

export interface PlaceOrderRequest {
  pair: string;
  side: 'buy' | 'sell';
  type: 'market' | 'limit';
  amount: number;
  price?: number;
}

export interface Order {
  id: string;
  pair: string;
  side: 'buy' | 'sell';
  type: 'market' | 'limit';
  status: 'open' | 'filled' | 'partial' | 'cancelled';
  amount: number;
  filled: number;
  price: number;
  avgPrice: number;
  total: number;
  fee: number;
  createdAt: string;
}

export interface Candle {
  time: number;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
}

export interface MarketTicker {
  pair: string;
  price: number;
  change24h: number;
  high24h: number;
  low24h: number;
  volume24h: number;
}

export interface LivePrice {
  symbol: string;
  lastPrice: number;
  bidPrice: number;
  askPrice: number;
  eventTime: string;
}

/** Live 24h rolling-window stats pushed by the market-data hub. */
export interface LiveTicker {
  symbol: string;
  lastPrice: number;
  priceChangePercent: number;
  highPrice: number;
  lowPrice: number;
  baseVolume: number;
  quoteVolume: number;
  eventTime: string;
}
