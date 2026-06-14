using Moq;
using TradingPairService.Application.Dto;
using TradingPairService.Application.Interfaces.Repositories;
using TradingPairService.Application.Services;
using TradingPairService.Domain.Entities;

namespace TradingPairService.Tests.Services
{
    public class TradingPairServiceTests
    {
        private readonly Mock<ITradingPairRepository> _mockRepository;
        private readonly Application.Services.TradingPairService _service;

        public TradingPairServiceTests()
        {
            _mockRepository = new Mock<ITradingPairRepository>();
            _service = new Application.Services.TradingPairService(_mockRepository.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ShouldReturnEmptyList_WhenNoTradingPairsExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAll())
                .ReturnsAsync(new List<TradingPair>());

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllTradingPairs_WhenPairsExist()
        {
            // Arrange
            var tradingPairs = new List<TradingPair>
            {
                new TradingPair
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTimeOffset.UtcNow,
                    CreatedBy = "test",
                    Symbol = "BTCUSDT",
                    BaseAsset = "BTC",
                    QuoteAsset = "USDT",
                    MinOrderQuantity = 0.001m,
                    MinOrderValue = 10m,
                    PricePrecision = 2,
                    QuantityPrecision = 8,
                    IsActive = true
                },
                new TradingPair
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTimeOffset.UtcNow,
                    CreatedBy = "test",
                    Symbol = "ETHUSDT",
                    BaseAsset = "ETH",
                    QuoteAsset = "USDT",
                    MinOrderQuantity = 0.01m,
                    MinOrderValue = 10m,
                    PricePrecision = 2,
                    QuantityPrecision = 6,
                    IsActive = true
                }
            };

            _mockRepository.Setup(r => r.GetAll())
                .ReturnsAsync(tradingPairs);

            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("BTCUSDT", result[0].Symbol);
            Assert.Equal("ETHUSDT", result[1].Symbol);
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }

        #endregion

        #region GetBySymbol Tests

        [Fact]
        public async Task GetBySymbol_ShouldReturnNull_WhenTradingPairDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync((TradingPair?)null);

            // Act
            var result = await _service.GetBySymbol("BTCUSDT");

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
        }

        [Fact]
        public async Task GetBySymbol_ShouldReturnTradingPair_WhenItExists()
        {
            // Arrange
            var tradingPair = new TradingPair
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "test",
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            };

            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync(tradingPair);

            // Act
            var result = await _service.GetBySymbol("BTCUSDT");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("BTCUSDT", result.Symbol);
            Assert.Equal("BTC", result.BaseAsset);
            Assert.Equal("USDT", result.QuoteAsset);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
        }

        [Fact]
        public async Task GetBySymbol_ShouldNormalizeSymbol_WhenCalledWithLowerCase()
        {
            // Arrange
            var tradingPair = new TradingPair
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "test",
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            };

            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync(tradingPair);

            // Act
            var result = await _service.GetBySymbol("btcusdt");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("BTCUSDT", result.Symbol);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
        }

        [Fact]
        public async Task GetBySymbol_ShouldTrimWhitespace_WhenSymbolHasSpaces()
        {
            // Arrange
            var tradingPair = new TradingPair
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "test",
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            };

            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync(tradingPair);

            // Act
            var result = await _service.GetBySymbol("  BTCUSDT  ");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("BTCUSDT", result.Symbol);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_ShouldCreateTradingPair_WhenRequestIsValid()
        {
            // Arrange
            var request = new CreateTradingPairRequest
            {
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8
            };

            _mockRepository.Setup(r => r.Exists("BTCUSDT"))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.Add(It.IsAny<TradingPair>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.Create(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("BTCUSDT", result.Symbol);
            Assert.Equal("BTC", result.BaseAsset);
            Assert.Equal("USDT", result.QuoteAsset);
            Assert.Equal(0.001m, result.MinOrderQuantity);
            Assert.Equal(10m, result.MinOrderValue);
            Assert.Equal(2, result.PricePrecision);
            Assert.Equal(8, result.QuantityPrecision);
            Assert.True(result.IsActive);

            _mockRepository.Verify(r => r.Exists("BTCUSDT"), Times.Once);
            _mockRepository.Verify(r => r.Add(It.Is<TradingPair>(tp => 
                tp.Symbol == "BTCUSDT" && 
                tp.BaseAsset == "BTC" && 
                tp.QuoteAsset == "USDT")), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldNormalizeAssets_WhenCalledWithLowerCase()
        {
            // Arrange
            var request = new CreateTradingPairRequest
            {
                BaseAsset = "btc",
                QuoteAsset = "usdt",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8
            };

            _mockRepository.Setup(r => r.Exists("BTCUSDT"))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.Add(It.IsAny<TradingPair>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.Create(request);

            // Assert
            Assert.Equal("BTCUSDT", result.Symbol);
            Assert.Equal("BTC", result.BaseAsset);
            Assert.Equal("USDT", result.QuoteAsset);

            _mockRepository.Verify(r => r.Add(It.Is<TradingPair>(tp => 
                tp.Symbol == "BTCUSDT" && 
                tp.BaseAsset == "BTC" && 
                tp.QuoteAsset == "USDT")), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldTrimAssets_WhenCalledWithWhitespace()
        {
            // Arrange
            var request = new CreateTradingPairRequest
            {
                BaseAsset = "  BTC  ",
                QuoteAsset = "  USDT  ",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8
            };

            _mockRepository.Setup(r => r.Exists("BTCUSDT"))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.Add(It.IsAny<TradingPair>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.Create(request);

            // Assert
            Assert.Equal("BTCUSDT", result.Symbol);
            Assert.Equal("BTC", result.BaseAsset);
            Assert.Equal("USDT", result.QuoteAsset);
        }

        [Fact]
        public async Task Create_ShouldThrowException_WhenBaseAndQuoteAreTheSame()
        {
            // Arrange
            var request = new CreateTradingPairRequest
            {
                BaseAsset = "BTC",
                QuoteAsset = "BTC",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.Create(request));

            Assert.Equal("Base asset and quote asset cannot be the same.", exception.Message);
            _mockRepository.Verify(r => r.Exists(It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(r => r.Add(It.IsAny<TradingPair>()), Times.Never);
        }

        [Fact]
        public async Task Create_ShouldThrowException_WhenTradingPairAlreadyExists()
        {
            // Arrange
            var request = new CreateTradingPairRequest
            {
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8
            };

            _mockRepository.Setup(r => r.Exists("BTCUSDT"))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.Create(request));

            Assert.Equal("Trading pair already exists.", exception.Message);
            _mockRepository.Verify(r => r.Exists("BTCUSDT"), Times.Once);
            _mockRepository.Verify(r => r.Add(It.IsAny<TradingPair>()), Times.Never);
        }

        #endregion

        #region Activate Tests

        [Fact]
        public async Task Activate_ShouldActivateTradingPair_WhenItExists()
        {
            // Arrange
            var tradingPair = new TradingPair
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "test",
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = false
            };

            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync(tradingPair);

            _mockRepository.Setup(r => r.Update(It.IsAny<TradingPair>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Activate("BTCUSDT");

            // Assert
            Assert.True(tradingPair.IsActive);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
            _mockRepository.Verify(r => r.Update(It.Is<TradingPair>(tp => 
                tp.Symbol == "BTCUSDT" && tp.IsActive == true)), Times.Once);
        }

        [Fact]
        public async Task Activate_ShouldThrowException_WhenTradingPairDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync((TradingPair?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.Activate("BTCUSDT"));

            Assert.Equal("Trading pair not found.", exception.Message);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
            _mockRepository.Verify(r => r.Update(It.IsAny<TradingPair>()), Times.Never);
        }

        [Fact]
        public async Task Activate_ShouldNormalizeSymbol_WhenCalledWithLowerCase()
        {
            // Arrange
            var tradingPair = new TradingPair
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "test",
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = false
            };

            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync(tradingPair);

            _mockRepository.Setup(r => r.Update(It.IsAny<TradingPair>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Activate("btcusdt");

            // Assert
            Assert.True(tradingPair.IsActive);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
        }

        #endregion

        #region Deactivate Tests

        [Fact]
        public async Task Deactivate_ShouldDeactivateTradingPair_WhenItExists()
        {
            // Arrange
            var tradingPair = new TradingPair
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "test",
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            };

            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync(tradingPair);

            _mockRepository.Setup(r => r.Update(It.IsAny<TradingPair>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Deactivate("BTCUSDT");

            // Assert
            Assert.False(tradingPair.IsActive);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
            _mockRepository.Verify(r => r.Update(It.Is<TradingPair>(tp => 
                tp.Symbol == "BTCUSDT" && tp.IsActive == false)), Times.Once);
        }

        [Fact]
        public async Task Deactivate_ShouldThrowException_WhenTradingPairDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync((TradingPair?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.Deactivate("BTCUSDT"));

            Assert.Equal("Trading pair not found.", exception.Message);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
            _mockRepository.Verify(r => r.Update(It.IsAny<TradingPair>()), Times.Never);
        }

        [Fact]
        public async Task Deactivate_ShouldNormalizeSymbol_WhenCalledWithLowerCase()
        {
            // Arrange
            var tradingPair = new TradingPair
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = "test",
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            };

            _mockRepository.Setup(r => r.GetBySymbol("BTCUSDT"))
                .ReturnsAsync(tradingPair);

            _mockRepository.Setup(r => r.Update(It.IsAny<TradingPair>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Deactivate("btcusdt");

            // Assert
            Assert.False(tradingPair.IsActive);
            _mockRepository.Verify(r => r.GetBySymbol("BTCUSDT"), Times.Once);
        }

        #endregion
    }
}
