export const environment = {
  production: true,
  apiBase: {
    auth: 'http://localhost:5003/api',
    wallet: 'http://localhost:5002/api',
    trade: 'https://trade-service.miniexchange.com/api/v1',
    transactions: 'https://transaction-service.miniexchange.com/api/v1',
    market: 'http://localhost:5205/api',
    user: 'https://user-service.miniexchange.com/api/v1',
  },
  marketHub: 'http://localhost:5205/hubs/market-data',
};
