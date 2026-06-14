using TradingService.Application.Dto;
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
            var service = new OrderService(repository);

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
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCreatedOrder()
        {
            var repository = new InMemoryOrderRepository();
            var service = new OrderService(repository);
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
        }

        [Fact]
        public async Task CreateAsync_RejectsLimitOrderWithoutPositivePrice()
        {
            var service = new OrderService(new InMemoryOrderRepository());

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
        public async Task DeleteAsync_ReturnsFalseWhenRepositoryCannotFindOrder()
        {
            var service = new OrderService(new InMemoryOrderRepository());

            var deleted = await service.DeleteAsync(Guid.NewGuid(), "trader");

            Assert.False(deleted);
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
        }
    }
}
