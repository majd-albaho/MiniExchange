using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nethereum.Signer;
using WalletService.Application.Dto;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Models;
using WalletService.Application.Services;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;
using static WalletService.Tests.TestEntities;

namespace WalletService.Tests;

public class UserWalletServiceTests
{
    [Fact]
    public async Task GetUserWallet_WhenWalletDoesNotExist_CreatesAndReturnsWallet()
    {
        var userId = Guid.NewGuid();
        UserWallet? createdWallet = null;
        var walletRepository = new Mock<IUserWalletRepository>(MockBehavior.Strict);
        walletRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserWallet?)null);
        walletRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<UserWallet>(), It.IsAny<CancellationToken>()))
            .Callback<UserWallet, CancellationToken>((wallet, _) => createdWallet = wallet)
            .ReturnsAsync((UserWallet wallet, CancellationToken _) =>
            {
                wallet.Id = 1;
                return wallet;
            });

        using var provider = BuildUserWalletServiceProvider(walletRepository);
        var service = provider.GetRequiredService<UserWalletService>();

        var wallet = await service.GetUserWallet(userId);

        Assert.Equal(1, wallet.Id);
        Assert.Equal(userId, wallet.UserId);
        Assert.Equal($"User {userId} Wallet", wallet.WalletName);
        Assert.NotNull(createdWallet);
        Assert.Equal(userId.ToString(), createdWallet.CreatedBy);
        walletRepository.Verify(repository => repository.CreateAsync(It.IsAny<UserWallet>(), It.IsAny<CancellationToken>()), Times.Once);
        walletRepository.VerifyAll();
    }

    [Fact]
    public async Task GetUserWallet_WhenWalletExists_ReturnsExistingWalletWithoutCreating()
    {
        var userId = Guid.NewGuid();
        var existingWallet = CreateUserWallet(15, userId, "Main wallet");
        var walletRepository = new Mock<IUserWalletRepository>(MockBehavior.Strict);
        walletRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWallet);

        using var provider = BuildUserWalletServiceProvider(walletRepository);
        var service = provider.GetRequiredService<UserWalletService>();

        var wallet = await service.GetUserWallet(userId);

        Assert.Equal(15, wallet.Id);
        Assert.Equal(userId, wallet.UserId);
        Assert.Equal("Main wallet", wallet.WalletName);
        walletRepository.Verify(repository => repository.CreateAsync(It.IsAny<UserWallet>(), It.IsAny<CancellationToken>()), Times.Never);
        walletRepository.VerifyAll();
    }

    [Fact]
    public async Task GetUserWalletAddress_WhenEthereumAddressDoesNotExist_CreatesAddressForWallet()
    {
        var userId = Guid.NewGuid();
        UserWalletAddress? createdAddress = null;
        var walletRepository = new Mock<IUserWalletRepository>(MockBehavior.Strict);
        walletRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUserWallet(21, userId, "Main wallet"));

        var addressRepository = new Mock<IUserWalletAddressRepository>(MockBehavior.Strict);
        addressRepository
            .Setup(repository => repository.GetByUserWalletId(21, CryptoNetworkType.Ethereum, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserWalletAddress?)null);
        addressRepository
            .Setup(repository => repository.AddAsync(It.IsAny<UserWalletAddress>(), It.IsAny<CancellationToken>()))
            .Callback<UserWalletAddress, CancellationToken>((address, _) => createdAddress = address)
            .ReturnsAsync((UserWalletAddress address, CancellationToken _) =>
            {
                address.Id = 100;
                return address;
            });

        using var provider = BuildUserWalletServiceProvider(walletRepository, addressRepository);
        var service = provider.GetRequiredService<UserWalletService>();

        var address = await service.GetUserWalletAddress(userId, CryptoNetworkType.Ethereum);

        Assert.Equal(100, address.Id);
        Assert.Equal(21, address.UserWalletId);
        Assert.Equal(CryptoNetworkType.Ethereum, address.CryptoNetworkType);
        Assert.False(string.IsNullOrWhiteSpace(address.PublicAddress));
        Assert.False(string.IsNullOrWhiteSpace(address.PrivateKey));
        Assert.Same(createdAddress, address);
        addressRepository.VerifyAll();
    }

    [Fact]
    public async Task GetUserWalletAddress_WhenNetworkIsUnsupported_ThrowsBeforeRepositoryCalls()
    {
        var walletRepository = new Mock<IUserWalletRepository>(MockBehavior.Strict);
        var addressRepository = new Mock<IUserWalletAddressRepository>(MockBehavior.Strict);

        using var provider = BuildUserWalletServiceProvider(walletRepository, addressRepository);
        var service = provider.GetRequiredService<UserWalletService>();

        await Assert.ThrowsAsync<NotSupportedException>(() => service.GetUserWalletAddress(Guid.NewGuid(), CryptoNetworkType.Bitcoin));

        walletRepository.Verify(repository => repository.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        addressRepository.Verify(repository => repository.AddAsync(It.IsAny<UserWalletAddress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckEthereumBalance_UsesEthereumWalletAddress()
    {
        var userId = Guid.NewGuid();
        var walletRepository = new Mock<IUserWalletRepository>(MockBehavior.Strict);
        walletRepository
            .Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUserWallet(31, userId, "Main wallet"));

        var addressRepository = new Mock<IUserWalletAddressRepository>(MockBehavior.Strict);
        addressRepository
            .Setup(repository => repository.GetByUserWalletId(31, CryptoNetworkType.Ethereum, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateWalletAddress(41, 31, "0xabc", "private-key"));

        var blockchainClient = new Mock<IWalletBlockchainClient>(MockBehavior.Strict);
        blockchainClient
            .Setup(client => client.GetEthereumBalanceAsync("0xabc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(12.34m);

        using var provider = BuildUserWalletServiceProvider(walletRepository, addressRepository, blockchainClient);
        var service = provider.GetRequiredService<UserWalletService>();

        var balance = await service.CheckEthereumBalance(userId);

        Assert.Equal(12.34m, balance);
        blockchainClient.VerifyAll();
    }

    private static ServiceProvider BuildUserWalletServiceProvider(
        Mock<IUserWalletRepository> walletRepository,
        Mock<IUserWalletAddressRepository>? addressRepository = null,
        Mock<IWalletBlockchainClient>? blockchainClient = null)
    {
        return new ServiceCollection()
            .AddSingleton(walletRepository.Object)
            .AddSingleton(addressRepository?.Object ?? Mock.Of<IUserWalletAddressRepository>())
            .AddSingleton(blockchainClient?.Object ?? Mock.Of<IWalletBlockchainClient>())
            .AddSingleton<ILogger<UserWalletService>>(NullLogger<UserWalletService>.Instance)
            .AddTransient<UserWalletService>()
            .BuildServiceProvider(validateScopes: true);
    }
}

public class WalletFundServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task LockFund_WhenAmountIsNotPositive_ThrowsBeforeDependenciesAreCalled(decimal amount)
    {
        var assetsRepository = new Mock<IUserWalletAssetsRepository>(MockBehavior.Strict);
        var userWalletService = new Mock<IUserWalletService>(MockBehavior.Strict);

        using var provider = BuildWalletFundServiceProvider(assetsRepository, userWalletService);
        var service = provider.GetRequiredService<WalletFundService>();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.LockFund(Guid.NewGuid(), 5, amount));

        userWalletService.Verify(service => service.GetUserWallet(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        assetsRepository.Verify(repository => repository.LockFundsAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LockFund_UsesWalletIdAndUserIdForRepositoryUpdate()
    {
        var userId = Guid.NewGuid();
        var assetsRepository = new Mock<IUserWalletAssetsRepository>(MockBehavior.Strict);
        assetsRepository
            .Setup(repository => repository.LockFundsAsync(77, 9, 3.5m, userId.ToString(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userWalletService = new Mock<IUserWalletService>(MockBehavior.Strict);
        userWalletService
            .Setup(service => service.GetUserWallet(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserWalletDto { Id = 77, UserId = userId, WalletName = "Main" });

        using var provider = BuildWalletFundServiceProvider(assetsRepository, userWalletService);
        var service = provider.GetRequiredService<WalletFundService>();

        await service.LockFund(userId, 9, 3.5m);

        assetsRepository.VerifyAll();
        userWalletService.VerifyAll();
    }

    [Fact]
    public async Task UnlockFund_UsesWalletIdAndUserIdForRepositoryUpdate()
    {
        var userId = Guid.NewGuid();
        var assetsRepository = new Mock<IUserWalletAssetsRepository>(MockBehavior.Strict);
        assetsRepository
            .Setup(repository => repository.UnlockFundsAsync(88, 10, 4.25m, userId.ToString(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userWalletService = new Mock<IUserWalletService>(MockBehavior.Strict);
        userWalletService
            .Setup(service => service.GetUserWallet(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserWalletDto { Id = 88, UserId = userId, WalletName = "Main" });

        using var provider = BuildWalletFundServiceProvider(assetsRepository, userWalletService);
        var service = provider.GetRequiredService<WalletFundService>();

        await service.UnlockFund(userId, 10, 4.25m);

        assetsRepository.VerifyAll();
        userWalletService.VerifyAll();
    }

    [Fact]
    public async Task CreditFund_ResolvesAssetByNameAndRecordsLedgerEntry()
    {
        var userId = Guid.NewGuid();

        var assetsRepository = new Mock<IUserWalletAssetsRepository>(MockBehavior.Strict);
        assetsRepository
            .Setup(repository => repository.CreditAsync(55, 3, 100m, userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserWalletAsset { Id = 1, UserWalletId = 55, AssetId = 3, Amount = 100m, LockedAmount = 0m, CreatedDate = DateTimeOffset.UtcNow, CreatedBy = "test" });

        var userWalletService = new Mock<IUserWalletService>(MockBehavior.Strict);
        userWalletService
            .Setup(service => service.GetUserWallet(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserWalletDto { Id = 55, UserId = userId, WalletName = "Main" });

        var assetService = new Mock<IAssetService>(MockBehavior.Strict);
        assetService
            .Setup(service => service.GetOrCreateByNameAsync("USDT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetDto { Id = 3, AssetName = "USDT" });

        WalletTransaction? recorded = null;
        var transactionRepository = new Mock<IWalletTransactionRepository>(MockBehavior.Strict);
        transactionRepository
            .Setup(repository => repository.RecordAsync(It.IsAny<WalletTransaction>(), It.IsAny<CancellationToken>()))
            .Callback<WalletTransaction, CancellationToken>((transaction, _) => recorded = transaction)
            .ReturnsAsync((WalletTransaction transaction, CancellationToken _) => transaction);

        using var provider = BuildWalletFundServiceProvider(assetsRepository, userWalletService, assetService, transactionRepository);
        var service = provider.GetRequiredService<WalletFundService>();

        await service.CreditFund(userId, "USDT", 100m);

        Assert.NotNull(recorded);
        Assert.Equal(WalletTransactionType.Credit, recorded!.Type);
        Assert.Equal(100m, recorded.Amount);
        Assert.Equal(100m, recorded.BalanceAfter);
        Assert.Null(recorded.ReferenceId);
        assetsRepository.VerifyAll();
        userWalletService.VerifyAll();
        assetService.VerifyAll();
        transactionRepository.VerifyAll();
    }

    private static ServiceProvider BuildWalletFundServiceProvider(
        Mock<IUserWalletAssetsRepository> assetsRepository,
        Mock<IUserWalletService> userWalletService,
        Mock<IAssetService>? assetService = null,
        Mock<IWalletTransactionRepository>? transactionRepository = null)
    {
        return new ServiceCollection()
            .AddSingleton(assetsRepository.Object)
            .AddSingleton(userWalletService.Object)
            .AddSingleton(assetService?.Object ?? Mock.Of<IAssetService>())
            .AddSingleton(transactionRepository?.Object ?? Mock.Of<IWalletTransactionRepository>())
            .AddSingleton<ILogger<WalletFundService>>(NullLogger<WalletFundService>.Instance)
            .AddTransient<WalletFundService>()
            .BuildServiceProvider(validateScopes: true);
    }
}

public class WalletSettlementServiceTests
{
    [Fact]
    public async Task SettleTradeAsync_ResolvesBothWalletsAndComputesQuoteAmount()
    {
        var tradeId = Guid.NewGuid();
        var buyerUserId = Guid.NewGuid();
        var sellerUserId = Guid.NewGuid();

        var userWalletService = new Mock<IUserWalletService>(MockBehavior.Strict);
        userWalletService
            .Setup(service => service.GetUserWallet(buyerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserWalletDto { Id = 10, UserId = buyerUserId, WalletName = "Buyer" });
        userWalletService
            .Setup(service => service.GetUserWallet(sellerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserWalletDto { Id = 20, UserId = sellerUserId, WalletName = "Seller" });

        TradeSettlementCommand? capturedCommand = null;
        var settlementRepository = new Mock<ITradeSettlementRepository>(MockBehavior.Strict);
        settlementRepository
            .Setup(repository => repository.SettleTradeAsync(It.IsAny<TradeSettlementCommand>(), It.IsAny<CancellationToken>()))
            .Callback<TradeSettlementCommand, CancellationToken>((command, _) => capturedCommand = command)
            .ReturnsAsync(true);

        using var provider = new ServiceCollection()
            .AddSingleton(userWalletService.Object)
            .AddSingleton(settlementRepository.Object)
            .AddSingleton<ILogger<WalletSettlementService>>(NullLogger<WalletSettlementService>.Instance)
            .AddTransient<WalletSettlementService>()
            .BuildServiceProvider(validateScopes: true);
        var service = provider.GetRequiredService<WalletSettlementService>();

        var result = await service.SettleTradeAsync(new TradeSettlementRequest
        {
            TradeId = tradeId,
            BuyerUserId = buyerUserId,
            SellerUserId = sellerUserId,
            BaseAssetId = 1,
            QuoteAssetId = 2,
            Quantity = 0.5m,
            Price = 65000m
        });

        Assert.True(result);
        Assert.NotNull(capturedCommand);
        Assert.Equal(tradeId, capturedCommand!.TradeId);
        Assert.Equal(10, capturedCommand.BuyerWalletId);
        Assert.Equal(20, capturedCommand.SellerWalletId);
        Assert.Equal(1, capturedCommand.BaseAssetId);
        Assert.Equal(2, capturedCommand.QuoteAssetId);
        Assert.Equal(0.5m, capturedCommand.Quantity);
        Assert.Equal(32500m, capturedCommand.QuoteAmount);
        userWalletService.VerifyAll();
        settlementRepository.VerifyAll();
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 0)]
    public async Task SettleTradeAsync_WhenQuantityOrPriceIsNotPositive_ThrowsBeforeResolvingWallets(decimal quantity, decimal price)
    {
        var userWalletService = new Mock<IUserWalletService>(MockBehavior.Strict);
        var settlementRepository = new Mock<ITradeSettlementRepository>(MockBehavior.Strict);

        using var provider = new ServiceCollection()
            .AddSingleton(userWalletService.Object)
            .AddSingleton(settlementRepository.Object)
            .AddSingleton<ILogger<WalletSettlementService>>(NullLogger<WalletSettlementService>.Instance)
            .AddTransient<WalletSettlementService>()
            .BuildServiceProvider(validateScopes: true);
        var service = provider.GetRequiredService<WalletSettlementService>();

        await Assert.ThrowsAsync<ArgumentException>(() => service.SettleTradeAsync(new TradeSettlementRequest
        {
            TradeId = Guid.NewGuid(),
            BuyerUserId = Guid.NewGuid(),
            SellerUserId = Guid.NewGuid(),
            BaseAssetId = 1,
            QuoteAssetId = 2,
            Quantity = quantity,
            Price = price
        }));

        userWalletService.Verify(service => service.GetUserWallet(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class WalletTransactionServiceTests
{
    [Fact]
    public async Task Send_Eth_UsesEthereumPrivateKeyAndSepoliaChain_AndRecordsWithdrawal()
    {
        var userId = Guid.NewGuid();
        var userWalletService = new Mock<IUserWalletService>(MockBehavior.Strict);
        userWalletService
            .Setup(service => service.GetUserWalletAddress(userId, CryptoNetworkType.Ethereum, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateWalletAddress(7, 8, "0xpublic", "private-key"));

        var blockchainClient = new Mock<IWalletBlockchainClient>(MockBehavior.Strict);
        blockchainClient
            .Setup(client => client.SendEthereumAsync("private-key", "0xrecipient", 1.25m, Chain.Sepolia, It.IsAny<CancellationToken>()))
            .ReturnsAsync("0xtx");

        var assetService = new Mock<IAssetService>(MockBehavior.Strict);
        assetService
            .Setup(s => s.GetOrCreateByNameAsync("ETH", false, CryptoNetworkType.Ethereum, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetDto { Id = 42, AssetName = "ETH", CryptoNetworkType = CryptoNetworkType.Ethereum, IsDemo = false });

        var walletFundService = new Mock<IWalletFundService>(MockBehavior.Strict);
        walletFundService
            .Setup(s => s.RecordWithdrawalAsync(userId, 42, 1.25m, "0xtx", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var provider = BuildWalletTransactionServiceProvider(userWalletService, blockchainClient, assetService, walletFundService);
        var service = provider.GetRequiredService<WalletTransactionService>();

        var transactionHash = await service.Send(userId, "ETH", "0xrecipient", 1.25m);

        Assert.Equal("0xtx", transactionHash);
        userWalletService.VerifyAll();
        blockchainClient.VerifyAll();
        assetService.VerifyAll();
        walletFundService.VerifyAll();
    }

    [Fact]
    public async Task Send_DemoToken_IsRejected()
    {
        var userId = Guid.NewGuid();
        var userWalletService = new Mock<IUserWalletService>(MockBehavior.Strict);
        var blockchainClient = new Mock<IWalletBlockchainClient>(MockBehavior.Strict);

        var assetService = new Mock<IAssetService>(MockBehavior.Strict);
        assetService
            .Setup(s => s.GetByNameAsync("DEMOBTC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssetDto { Id = 9, AssetName = "DEMOBTC", CryptoNetworkType = CryptoNetworkType.None, IsDemo = true });

        var walletFundService = new Mock<IWalletFundService>(MockBehavior.Strict);

        using var provider = BuildWalletTransactionServiceProvider(userWalletService, blockchainClient, assetService, walletFundService);
        var service = provider.GetRequiredService<WalletTransactionService>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.Send(userId, "DEMOBTC", "0xrecipient", 1m));

        blockchainClient.Verify(
            c => c.SendEthereumAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<Chain>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTransactionDetails_ReturnsBlockchainClientResult()
    {
        var blockchainClient = new Mock<IWalletBlockchainClient>(MockBehavior.Strict);
        blockchainClient
            .Setup(client => client.GetTransactionDetailsAsync("0xtx", It.IsAny<CancellationToken>()))
            .ReturnsAsync("details");

        using var provider = BuildWalletTransactionServiceProvider(
            new Mock<IUserWalletService>(MockBehavior.Strict),
            blockchainClient,
            new Mock<IAssetService>(MockBehavior.Strict),
            new Mock<IWalletFundService>(MockBehavior.Strict));
        var service = provider.GetRequiredService<WalletTransactionService>();

        var result = await service.GetTransactionDetails("0xtx");

        Assert.Equal("details", result);
        blockchainClient.VerifyAll();
    }

    private static ServiceProvider BuildWalletTransactionServiceProvider(
        Mock<IUserWalletService> userWalletService,
        Mock<IWalletBlockchainClient> blockchainClient,
        Mock<IAssetService> assetService,
        Mock<IWalletFundService> walletFundService)
    {
        return new ServiceCollection()
            .AddSingleton(userWalletService.Object)
            .AddSingleton(blockchainClient.Object)
            .AddSingleton(assetService.Object)
            .AddSingleton(walletFundService.Object)
            .AddSingleton<ILogger<WalletTransactionService>>(NullLogger<WalletTransactionService>.Instance)
            .AddTransient<WalletTransactionService>()
            .BuildServiceProvider(validateScopes: true);
    }
}

internal static class TestEntities
{
    public static UserWallet CreateUserWallet(long id, Guid userId, string walletName)
    {
        return new UserWallet
        {
            Id = id,
            UserId = userId,
            WalletName = walletName,
            CreatedDate = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
    }

    public static UserWalletAddress CreateWalletAddress(long id, long userWalletId, string publicAddress, string privateKey)
    {
        return new UserWalletAddress
        {
            Id = id,
            UserWalletId = userWalletId,
            CryptoNetworkType = CryptoNetworkType.Ethereum,
            PublicAddress = publicAddress,
            PrivateKey = privateKey,
            CreatedDate = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
    }
}
