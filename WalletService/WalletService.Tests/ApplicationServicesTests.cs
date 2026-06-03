using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nethereum.Signer;
using WalletService.Application.Dto;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
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

    private static ServiceProvider BuildWalletFundServiceProvider(
        Mock<IUserWalletAssetsRepository> assetsRepository,
        Mock<IUserWalletService> userWalletService)
    {
        return new ServiceCollection()
            .AddSingleton(assetsRepository.Object)
            .AddSingleton(userWalletService.Object)
            .AddSingleton<ILogger<WalletFundService>>(NullLogger<WalletFundService>.Instance)
            .AddTransient<WalletFundService>()
            .BuildServiceProvider(validateScopes: true);
    }
}

public class WalletTransactionServiceTests
{
    [Fact]
    public async Task SendEthereum_UsesEthereumPrivateKeyAndSepoliaChain()
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

        using var provider = BuildWalletTransactionServiceProvider(userWalletService, blockchainClient);
        var service = provider.GetRequiredService<WalletTransactionService>();

        var transactionHash = await service.SendEthereum(userId, "0xrecipient", 1.25m);

        Assert.Equal("0xtx", transactionHash);
        userWalletService.VerifyAll();
        blockchainClient.VerifyAll();
    }

    [Fact]
    public async Task GetTransactionDetails_ReturnsBlockchainClientResult()
    {
        var blockchainClient = new Mock<IWalletBlockchainClient>(MockBehavior.Strict);
        blockchainClient
            .Setup(client => client.GetTransactionDetailsAsync("0xtx", It.IsAny<CancellationToken>()))
            .ReturnsAsync("details");

        using var provider = BuildWalletTransactionServiceProvider(new Mock<IUserWalletService>(MockBehavior.Strict), blockchainClient);
        var service = provider.GetRequiredService<WalletTransactionService>();

        var result = await service.GetTransactionDetails("0xtx");

        Assert.Equal("details", result);
        blockchainClient.VerifyAll();
    }

    private static ServiceProvider BuildWalletTransactionServiceProvider(
        Mock<IUserWalletService> userWalletService,
        Mock<IWalletBlockchainClient> blockchainClient)
    {
        return new ServiceCollection()
            .AddSingleton(userWalletService.Object)
            .AddSingleton(blockchainClient.Object)
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
