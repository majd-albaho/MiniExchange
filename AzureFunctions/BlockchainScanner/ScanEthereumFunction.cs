using BlockchainScanner.Services;
using Microsoft.Azure.Functions.Worker;

namespace BlockchainScanner
{
    public class ScanEthereumFunction
    {
        private readonly EthereumScanner _scanner;

        public ScanEthereumFunction(EthereumScanner scanner)
        {
            _scanner = scanner;
        }

        [Function("ScanEthereum")]
        public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo timer)
        {
            await _scanner.ScanAsync();
        }
    }
}
