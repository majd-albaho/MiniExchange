export const environment = {
  production: true,
  apiBase: {
    auth: 'http://localhost:5003/api',
    wallet: 'http://localhost:5002/api',
    trade: 'http://localhost:5207/api',
    pairs: 'http://localhost:5208/api',
    transactions: 'http://localhost:5002/api',
    market: 'http://localhost:5205/api',
    user: 'https://user-service.miniexchange.com/api/v1',
  },
  marketHub: 'http://localhost:5205/hubs/market-data',
};
