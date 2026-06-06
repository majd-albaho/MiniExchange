using BlockchainScanner.Entites;
using Microsoft.EntityFrameworkCore;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Numerics;

namespace BlockchainScanner.Services
{
    public class EthereumScanner
    {
        private readonly AppDbContext _db;
        private readonly Web3 _web3;

        public EthereumScanner(AppDbContext db)
        {
            _db = db;
            _web3 = new Web3(Environment.GetEnvironmentVariable("AlchemyRpcUrl"));
        }

        public async Task ScanAsync()
        {
            var state = await _db.BlockChainStates.FirstOrDefaultAsync(x => x.Network == "ETH");

            var lastProcessed = state?.LastProcessedBlock ?? 0;
            long latestBlock = (long)(await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value;

            // Wait for confirmations
            latestBlock -= 12;
            if (latestBlock <= lastProcessed)
                return;

            var trackedWallets = (await _db.RegisteredWallets.Select(x => x.PublicAddress.ToLower()).ToListAsync()).ToHashSet();
            for (var block = lastProcessed + 1; block <= latestBlock; block++)
            {
                await ProcessBlock(block, trackedWallets);
            }

            if (state == null)
            {
                state = new BlockChainState { Network = "ETH" };
                _db.BlockChainStates.Add(state);
            }

            state.LastProcessedBlock = latestBlock;
            await _db.SaveChangesAsync();
        }

        private async Task ProcessBlock(BigInteger blockNumber, HashSet<string> trackedWallets)
        {
            var block = await _web3.Eth.Blocks
                .GetBlockWithTransactionsByNumber
                .SendRequestAsync(new BlockParameter(new Nethereum.Hex.HexTypes.HexBigInteger(blockNumber)));

            foreach (var tx in block.Transactions)
            {
                string from = tx.From?.ToLower() ?? "";
                string to = tx.To?.ToLower() ?? "";

                bool incoming = trackedWallets.Contains(to);
                bool outgoing = trackedWallets.Contains(from);

                if (!incoming && !outgoing)
                    continue;

                bool exists = await _db.BlockchainTransactions.AnyAsync(x => x.TransactionHash == tx.TransactionHash);
                if (exists)
                    continue;

                decimal amount = Web3.Convert.FromWei(tx.Value);

                _db.BlockchainTransactions.Add(new BlockchainTransaction
                {
                    TransactionHash = tx.TransactionHash,
                    BlockNumber = blockNumber,
                    TransactionType = incoming
                                ? TransactionType.Deposit
                                : TransactionType.Withdrawal,

                    From = from,
                    To = to,
                    Amount = amount,
                    Asset = "ETH",
                    Confirmed = true,
                    TransactionDateTime = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
