export interface WalletAsset {
  id: string;
  symbol: string;
  name: string;
  network: string;
  balance: number;
  balanceUSDT: number;
  depositAddress: string;
  logoUrl: string;
  change24h: number;
  price: number;
}

export interface WalletOverview {
  totalBalanceUSDT: number;
  totalChange24h: number;
  assets: WalletAsset[];
}

export interface SendRequest {
  fromSymbol: string;
  toAddress: string;
  amount: number;
  network: string;
  pin: string;
  memo?: string;
}

export interface ReceiveInfo {
  symbol: string;
  network: string;
  address: string;
  memo?: string;
  minDeposit: number;
  confirmations: number;
}
