using Microsoft.Extensions.Logging.Abstractions;
using Nethereum.Signer;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Services;
using WalletService.Domain.Entities;

namespace WalletService.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task LockFund_IncreasesLockedBalance_WhenAvailableBalanceCoversAmount() {
            var userId = Guid.NewGuid();
            var repository = new InMemoryUserWalletRepository(CreateWallet(userId, lockedBalance: 2m));
            var service = new UserWalletService(repository, new FixedBalanceBlockchainClient(10m), NullLogger<UserWalletService>.Instance);

            var lockedBalance = await service.LockFund(userId, 3m);

            Assert.Equal(5m, lockedBalance);
            Assert.Equal(5m, repository.StoredWallet.LockedBalance);
        }

        [Fact]
        public async Task LockFund_Rejects_WhenExistingLocksLeaveInsufficientAvailableBalance() {
            var userId = Guid.NewGuid();
            var repository = new InMemoryUserWalletRepository(CreateWallet(userId, lockedBalance: 8m));
            var service = new UserWalletService(repository, new FixedBalanceBlockchainClient(10m), NullLogger<UserWalletService>.Instance);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.LockFund(userId, 3m));

            Assert.Equal("Insufficient available balance to lock funds", exception.Message);
            Assert.Equal(8m, repository.StoredWallet.LockedBalance);
        }

        [Fact]
        public async Task UnlockFund_DecreasesLockedBalance_WhenAmountIsLocked() {
            var userId = Guid.NewGuid();
            var repository = new InMemoryUserWalletRepository(CreateWallet(userId, lockedBalance: 5m));
            var service = new UserWalletService(repository, new FixedBalanceBlockchainClient(0m), NullLogger<UserWalletService>.Instance);

            var lockedBalance = await service.UnlockFund(userId, 2m);

            Assert.Equal(3m, lockedBalance);
            Assert.Equal(3m, repository.StoredWallet.LockedBalance);
        }

        [Fact]
        public async Task UnlockFund_Rejects_WhenAmountExceedsLockedBalance() {
            var userId = Guid.NewGuid();
            var repository = new InMemoryUserWalletRepository(CreateWallet(userId, lockedBalance: 1m));
            var service = new UserWalletService(repository, new FixedBalanceBlockchainClient(0m), NullLogger<UserWalletService>.Instance);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UnlockFund(userId, 2m));

            Assert.Equal("Insufficient locked balance to unlock funds", exception.Message);
            Assert.Equal(1m, repository.StoredWallet.LockedBalance);
        }

        [Fact]
        public async Task LockFund_Rejects_NonPositiveAmount() {
            var userId = Guid.NewGuid();
            var repository = new InMemoryUserWalletRepository(CreateWallet(userId, lockedBalance: 0m));
            var service = new UserWalletService(repository, new FixedBalanceBlockchainClient(10m), NullLogger<UserWalletService>.Instance);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.LockFund(userId, 0m));
        }

        private static UserWallet CreateWallet(Guid userId, decimal lockedBalance) {
            return new UserWallet {
                Id = 1,
                UserId = userId,
                Address = "0x0000000000000000000000000000000000000000",
                PrivateKey = "private-key",
                LockedBalance = lockedBalance,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedBy = userId.ToString(),
                ModifiedDate = DateTimeOffset.UtcNow,
            };
        }

        private sealed class FixedBalanceBlockchainClient : IWalletBlockchainClient
        {
            private readonly decimal _balance;

            public FixedBalanceBlockchainClient(decimal balance) {
                _balance = balance;
            }

            public Task<decimal> GetEtherBalanceAsync(string address, CancellationToken cancellationToken = default) {
                return Task.FromResult(_balance);
            }

            public Task<string> SendEtheriumAsync(string privateKey, string recipientAddress, decimal amount, Chain chain, CancellationToken cancellationToken = default) {
                throw new NotSupportedException();
            }

            public Task<string> GetTransactionDetailsAsync(string transactionId, CancellationToken cancellationToken = default) {
                throw new NotSupportedException();
            }
        }

        private sealed class InMemoryUserWalletRepository : IUserWalletRepository
        {
            private UserWallet? _wallet;

            public InMemoryUserWalletRepository(UserWallet wallet) {
                _wallet = wallet;
            }

            public UserWallet StoredWallet => _wallet ?? throw new InvalidOperationException("Wallet was deleted");

            public Task<UserWallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) {
                return Task.FromResult(_wallet?.UserId == userId ? Clone(_wallet) : null);
            }

            public Task<UserWallet> CreateAsync(UserWallet userWallet, CancellationToken cancellationToken = default) {
                userWallet.Id = userWallet.Id == default ? 1 : userWallet.Id;
                _wallet = Clone(userWallet);
                return Task.FromResult(userWallet);
            }

            public Task<bool> TryLockFundsAsync(long walletId, decimal amount, decimal totalBalance, string modifiedBy, CancellationToken cancellationToken = default) {
                if (_wallet == null || _wallet.Id != walletId || _wallet.LockedBalance > totalBalance - amount) {
                    return Task.FromResult(false);
                }

                _wallet.LockedBalance += amount;
                _wallet.ModifiedBy = modifiedBy;
                _wallet.ModifiedDate = DateTimeOffset.UtcNow;
                return Task.FromResult(true);
            }

            public Task<bool> TryUnlockFundsAsync(long walletId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default) {
                if (_wallet == null || _wallet.Id != walletId || _wallet.LockedBalance < amount) {
                    return Task.FromResult(false);
                }

                _wallet.LockedBalance -= amount;
                _wallet.ModifiedBy = modifiedBy;
                _wallet.ModifiedDate = DateTimeOffset.UtcNow;
                return Task.FromResult(true);
            }

            public Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default) {
                if (_wallet?.UserId != userId) {
                    return Task.FromResult(false);
                }

                _wallet = null;
                return Task.FromResult(true);
            }

            private static UserWallet Clone(UserWallet wallet) {
                return new UserWallet {
                    Id = wallet.Id,
                    UserId = wallet.UserId,
                    Address = wallet.Address,
                    PrivateKey = wallet.PrivateKey,
                    LockedBalance = wallet.LockedBalance,
                    CreatedBy = wallet.CreatedBy,
                    CreatedDate = wallet.CreatedDate,
                    ModifiedBy = wallet.ModifiedBy,
                    ModifiedDate = wallet.ModifiedDate,
                    DeletedBy = wallet.DeletedBy,
                    DeletedDate = wallet.DeletedDate,
                };
            }
        }
    }
}
