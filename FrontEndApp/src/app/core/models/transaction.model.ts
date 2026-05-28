export type TransactionType = 'send' | 'receive' | 'buy' | 'sell';
export type TransactionStatus = 'pending' | 'completed' | 'failed' | 'cancelled' | 'filled';

export interface Transaction {
  id: string;
  type: TransactionType;
  status: TransactionStatus;
  symbol: string;
  amount: number;
  amountUSDT: number;
  fee: number;
  feeSymbol: string;
  fromAddress?: string;
  toAddress?: string;
  txHash?: string;
  network?: string;
  createdAt: string;
  updatedAt: string;
  pair?: string;
  price?: number;
}

export interface TransactionFilter {
  type?: TransactionType | 'all';
  status?: TransactionStatus | 'all';
  symbol?: string;
  startDate?: string;
  endDate?: string;
  page: number;
  pageSize: number;
}

export interface PaginatedTransactions {
  items: Transaction[];
  total: number;
  page: number;
  pageSize: number;
}
