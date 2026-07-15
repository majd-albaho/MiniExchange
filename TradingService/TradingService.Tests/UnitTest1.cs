using Microsoft.Extensions.Logging.Abstractions;
using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Clients;
using TradingService.Application.Interfaces.Repositories;
using TradingService.Application.Services;
using TradingService.Domain.Entities;

namespace TradingService.Tests
{
    public sealed class OrderServiceTests
    {
        [Fact]
        public async Task CreateAsync_NormalizesPairSymbolAndInitializesPendingOrder()
        {
            var repository = new InMemoryOrderRepository();
            var (service, _, walletClient, _) = CreateService(repository);

            var response = await service.CreateAsync(new CreateOrderRequest
            {
                UserId = Guid.NewGuid(),
                PairSymbol = " btc-usdt ",
                Side = OrderSide.Buy,
                Type = OrderType.Limit,
                Price = 65000m,
                Quantity = 0.5m,
                CreatedBy = "trader"
            });

            Assert.NotEqual(Guid.Empty, response.Id);
            Assert.Equal("BTC-USDT", response.PairSymbol);
            Assert.Equal(OrderStatus.Pending, response.Status);
            Assert.Equal(0m, response.FilledQuantity);
            Assert.Equal(response.Id, repository.CreatedOrder!.Id);
            Assert.Equal("trader", repository.CreatedOrder.CreatedBy);

            var lockCall = Assert.Single(walletClient.LockedCalls);
            Assert.Equal(walletClient.AssetId("USDT"), lockCall.AssetId);
            Assert.Equal(32500m, lockCall.Amount);
        }

        [Fact]
        public async Task CreateAsync_SellOrderLocksBaseAssetQuantityRegardlessOfPrice()
        {
            var repository = new InMemoryOrderRepository();
            var (service, _, walletClient, _) = CreateService(repository);

            var created = await service.CreateAsync(new CreateOrderRequest
            {
                UserId = Guid.NewGuid(),
                PairSymbol = "ETH-USDT",
                Side = OrderSide.Sell,
                Type = OrderType.Market,
                Price = 0m,
                Quantity = 2m
            });

            var found = await service.GetByIdAsync(created.Id);

            Assert.NotNull(found);
            Assert.Equal(created.Id, found.Id);
            Assert.Equal("ETH-USDT", found.PairSymbol);

            var lockCall = Assert.Single(walletClient.LockedCalls);
            Assert.Equal(walletClient.AssetId("ETH"), lockCall.AssetId);
            Assert.Equal(2m, lockCall.Amount);
        }

        [Fact]
        public async Task CreateAsync_RejectsLimitOrderWithoutPositivePrice()
        {
            var (service, _, _, _) = CreateService(new InMemoryOrderRepository());

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateOrderRequest
            {
                UserId = Guid.NewGuid(),
                PairSymbol = "BTC-USDT",
                Side = OrderSide.Buy,
                Type = OrderType.Limit,
                Price = 0m,
                Quantity = 1m
            }));
        }

        [Fact]
        public async Task CreateAsync_RejectsUnknownTradingPair()
        {
            var (service, _, walletClient, _) = CreateService(new InMemoryOrderRepository());

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateOrderRequest
            {
                UserId = Guid.NewGuid(),
                PairSymbol = "DOGE-USDT",
                Side = OrderSide.Buy,
                Type = OrderType.Limit,
                Price = 1m,
                Quantity = 1m
            }));

            Assert.Empty(walletClient.LockedCalls);
        }

        [Fact]
        public async Task CreateAsync_RejectsMarketOrderWhenNoLivePriceIsCached()
        {
            var (service, _, walletClient, _) = CreateService(new InMemoryOrderRepository());

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateOrderRequest
            {
                UserId = Guid.NewGuid(),
                PairSymbol = "BTC-USDT",
                Side = OrderSide.Buy,
                Type = OrderType.Market,
                Price = 0m,
                Quantity = 1m
            }));

            Assert.Empty(walletClient.LockedCalls);
        }

        [Fact]
        public async Task CreateAsync_WhenLockFundsFails_DoesNotPersistTheOrder()
        {
            var repository = new InMemoryOrderRepository();
            var (service, _, walletClient, _) = CreateService(repository);
            walletClient.ThrowOnLock = true;

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateOrderRequest
            {
                UserId = Guid.NewGuid(),
                PairSymbol = "BTC-USDT",
                Side = OrderSide.Buy,
                Type = OrderType.Limit,
                Price = 65000m,
                Quantity = 0.5m
            }));

            Assert.Null(repository.CreatedOrder);
        }

        [Fact]
        public async Task DeleteAsync_UnlocksRemainingReservedBalance()
        {
            var repository = new InMemoryOrderRepository();
            var (service, _, walletClient, _) = CreateService(repository);

            var created = await service.CreateAsync(new CreateOrderRequest
            {
                UserId = Guid.NewGuid(),
                PairSymbol = "BTC-USDT",
                Side = OrderSide.Buy,
                Type = OrderType.Limit,
                Price = 65000m,
                Quantity = 0.5m
            });

            var deleted = await service.DeleteAsync(created.Id, "trader");

            Assert.True(deleted);
            var unlockCall = Assert.Single(walletClient.UnlockedCalls);
            Assert.Equal(walletClient.AssetId("USDT"), unlockCall.AssetId);
            Assert.Equal(32500m, unlockCall.Amount);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalseWhenRepositoryCannotFindOrder()
        {
            var (service, _, _, _) = CreateService(new InMemoryOrderRepository());

            var deleted = await service.DeleteAsync(Guid.NewGuid(), "trader");

            Assert.False(deleted);
        }

        private static (OrderService Service, StubTradingPairClient PairClient, StubWalletServiceClient WalletClient, StubMarketDataClient MarketDataClient) CreateService(IOrderRepository repository)
        {
            var pairClient = new StubTradingPairClient();
            pairClient.AddPair(new TradingPairInfo
            {
                Symbol = "BTC-USDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0m,
                MinOrderValue = 0m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            });
            pairClient.AddPair(new TradingPairInfo
            {
                Symbol = "ETH-USDT",
                BaseAsset = "ETH",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0m,
                MinOrderValue = 0m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            });

            var marketDataClient = new StubMarketDataClient();
            marketDataClient.SetPrice("ETH-USDT", 3000m);

            var walletClient = new StubWalletServiceClient();
            var service = new OrderService(repository, new NoOpMatchingEngineClient(), pairClient, walletClient, marketDataClient, NullLogger<OrderService>.Instance);

            return (service, pairClient, walletClient, marketDataClient);
        }

        private sealed class InMemoryOrderRepository : IOrderRepository
        {
            public Order? CreatedOrder { get; private set; }

            public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(CreatedOrder?.Id == id ? CreatedOrder : null);
            }

            public Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
            {
                CreatedOrder = order;
                return Task.FromResult(order);
            }

            public Task<bool> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(CreatedOrder?.Id == id);
            }

            public Task<bool> UpdateAsync(Order order, CancellationToken cancellationToken = default)
            {
                if (CreatedOrder?.Id == order.Id)
                {
                    CreatedOrder = order;
                }
                return Task.FromResult(CreatedOrder?.Id == order.Id);
            }

            public Task<IReadOnlyList<Order>> GetOpenByUserAsync(Guid userId, CancellationToken cancellationToken = default)
            {
                IReadOnlyList<Order> orders = CreatedOrder is not null && CreatedOrder.UserId == userId
                    ? new[] { CreatedOrder }
                    : Array.Empty<Order>();
                return Task.FromResult(orders);
            }
        }

        private sealed class NoOpMatchingEngineClient : IMatchingEngineClient
        {
            public Task<bool> SubmitOrderAsync(Order order, CancellationToken cancellationToken = default) => Task.FromResult(true);

            public Task<bool> CancelOrderAsync(Guid orderId, string pairSymbol, CancellationToken cancellationToken = default) => Task.FromResult(true);

            public Task<OrderBookSnapshotDto> GetOrderBookAsync(string pairSymbol, int depth, CancellationToken cancellationToken = default)
                => Task.FromResult(new OrderBookSnapshotDto { PairSymbol = pairSymbol });
        }

        private sealed class StubTradingPairClient : ITradingPairClient
        {
            private readonly Dictionary<string, TradingPairInfo> _pairs = new();

            public void AddPair(TradingPairInfo pair) => _pairs[pair.Symbol] = pair;

            public Task<TradingPairInfo?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_pairs.TryGetValue(symbol, out var pair) ? pair : null);
            }
        }

        private sealed class StubMarketDataClient : IMarketDataClient
        {
            private readonly Dictionary<string, decimal> _prices = new();

            public void SetPrice(string symbol, decimal price) => _prices[symbol] = price;

            public Task<decimal?> GetLatestPriceAsync(string symbol, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_prices.TryGetValue(symbol, out var price) ? price : (decimal?)null);
            }
        }

        private sealed class StubWalletServiceClient : IWalletServiceClient
        {
            private readonly Dictionary<string, long> _assetIds = new();
            private long _nextAssetId = 1;

            public bool ThrowOnLock { get; set; }
            public List<(Guid UserId, long AssetId, decimal Amount)> LockedCalls { get; } = new();
            public List<(Guid UserId, long AssetId, decimal Amount)> UnlockedCalls { get; } = new();

            public long AssetId(string assetName) => _assetIds[assetName];

            public Task<long> GetOrCreateAssetAsync(string assetName, CancellationToken cancellationToken = default)
            {
                if (!_assetIds.TryGetValue(assetName, out var id))
                {
                    id = _nextAssetId++;
                    _assetIds[assetName] = id;
                }

                return Task.FromResult(id);
            }

            public Task LockFundsAsync(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default)
            {
                if (ThrowOnLock)
                {
                    throw new InvalidOperationException("Insufficient available balance to lock funds");
                }

                LockedCalls.Add((userId, assetId, amount));
                return Task.CompletedTask;
            }

            public Task UnlockFundsAsync(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default)
            {
                UnlockedCalls.Add((userId, assetId, amount));
                return Task.CompletedTask;
            }

            public Task<bool> SettleTradeAsync(
                Guid tradeId, Guid buyerUserId, Guid sellerUserId, long baseAssetId, long quoteAssetId, decimal quantity, decimal price, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(true);
            }
        }
    }
}
