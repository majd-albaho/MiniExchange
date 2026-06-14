using Microsoft.AspNetCore.Mvc;
using Moq;
using TradingPairService.Api.Controllers;
using TradingPairService.Application.Dto;
using TradingPairService.Application.Interfaces.Services;

namespace TradingPairService.Tests.Controllers
{
    public class TradingPairsControllerTests
    {
        private readonly Mock<ITradingPairService> _mockService;
        private readonly TradingPairsController _controller;

        public TradingPairsControllerTests()
        {
            _mockService = new Mock<ITradingPairService>();
            _controller = new TradingPairsController(_mockService.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ShouldReturnOkWithEmptyList_WhenNoTradingPairsExist()
        {
            // Arrange
            _mockService.Setup(s => s.GetAll())
                .ReturnsAsync(new List<TradingPairResponse>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var pairs = Assert.IsType<List<TradingPairResponse>>(okResult.Value);
            Assert.Empty(pairs);
            _mockService.Verify(s => s.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWithTradingPairs_WhenPairsExist()
        {
            // Arrange
            var tradingPairs = new List<TradingPairResponse>
            {
                new TradingPairResponse
                {
                    Symbol = "BTCUSDT",
                    BaseAsset = "BTC",
                    QuoteAsset = "USDT",
                    MinOrderQuantity = 0.001m,
                    MinOrderValue = 10m,
                    PricePrecision = 2,
                    QuantityPrecision = 8,
                    IsActive = true
                },
                new TradingPairResponse
                {
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

            _mockService.Setup(s => s.GetAll())
                .ReturnsAsync(tradingPairs);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var pairs = Assert.IsType<List<TradingPairResponse>>(okResult.Value);
            Assert.Equal(2, pairs.Count);
            Assert.Equal("BTCUSDT", pairs[0].Symbol);
            Assert.Equal("ETHUSDT", pairs[1].Symbol);
            _mockService.Verify(s => s.GetAll(), Times.Once);
        }

        #endregion

        #region GetBySymbol Tests

        [Fact]
        public async Task GetBySymbol_ShouldReturnNotFound_WhenTradingPairDoesNotExist()
        {
            // Arrange
            _mockService.Setup(s => s.GetBySymbol("BTCUSDT"))
                .ReturnsAsync((TradingPairResponse?)null);

            // Act
            var result = await _controller.GetBySymbol("BTCUSDT");

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockService.Verify(s => s.GetBySymbol("BTCUSDT"), Times.Once);
        }

        [Fact]
        public async Task GetBySymbol_ShouldReturnOkWithTradingPair_WhenItExists()
        {
            // Arrange
            var tradingPair = new TradingPairResponse
            {
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            };

            _mockService.Setup(s => s.GetBySymbol("BTCUSDT"))
                .ReturnsAsync(tradingPair);

            // Act
            var result = await _controller.GetBySymbol("BTCUSDT");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var pair = Assert.IsType<TradingPairResponse>(okResult.Value);
            Assert.Equal("BTCUSDT", pair.Symbol);
            Assert.Equal("BTC", pair.BaseAsset);
            Assert.Equal("USDT", pair.QuoteAsset);
            _mockService.Verify(s => s.GetBySymbol("BTCUSDT"), Times.Once);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenRequestIsValid()
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

            var response = new TradingPairResponse
            {
                Symbol = "BTCUSDT",
                BaseAsset = "BTC",
                QuoteAsset = "USDT",
                MinOrderQuantity = 0.001m,
                MinOrderValue = 10m,
                PricePrecision = 2,
                QuantityPrecision = 8,
                IsActive = true
            };

            _mockService.Setup(s => s.Create(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(TradingPairsController.GetBySymbol), createdResult.ActionName);

            var routeValues = createdResult.RouteValues;
            Assert.NotNull(routeValues);
            Assert.True(routeValues.ContainsKey("symbol"));
            Assert.Equal("BTCUSDT", routeValues["symbol"]);

            var pair = Assert.IsType<TradingPairResponse>(createdResult.Value);
            Assert.Equal("BTCUSDT", pair.Symbol);
            Assert.Equal("BTC", pair.BaseAsset);
            Assert.Equal("USDT", pair.QuoteAsset);

            _mockService.Verify(s => s.Create(request), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldPropagateException_WhenServiceThrows()
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

            _mockService.Setup(s => s.Create(request))
                .ThrowsAsync(new InvalidOperationException("Base asset and quote asset cannot be the same."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Create(request));

            Assert.Equal("Base asset and quote asset cannot be the same.", exception.Message);
            _mockService.Verify(s => s.Create(request), Times.Once);
        }

        #endregion

        #region Activate Tests

        [Fact]
        public async Task Activate_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            _mockService.Setup(s => s.Activate("BTCUSDT"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Activate("BTCUSDT");

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.Activate("BTCUSDT"), Times.Once);
        }

        [Fact]
        public async Task Activate_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            _mockService.Setup(s => s.Activate("NONEXISTENT"))
                .ThrowsAsync(new InvalidOperationException("Trading pair not found."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Activate("NONEXISTENT"));

            Assert.Equal("Trading pair not found.", exception.Message);
            _mockService.Verify(s => s.Activate("NONEXISTENT"), Times.Once);
        }

        #endregion

        #region Deactivate Tests

        [Fact]
        public async Task Deactivate_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            _mockService.Setup(s => s.Deactivate("BTCUSDT"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Deactivate("BTCUSDT");

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.Deactivate("BTCUSDT"), Times.Once);
        }

        [Fact]
        public async Task Deactivate_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            _mockService.Setup(s => s.Deactivate("NONEXISTENT"))
                .ThrowsAsync(new InvalidOperationException("Trading pair not found."));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Deactivate("NONEXISTENT"));

            Assert.Equal("Trading pair not found.", exception.Message);
            _mockService.Verify(s => s.Deactivate("NONEXISTENT"), Times.Once);
        }

        #endregion
    }
}
