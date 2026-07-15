using TradingPairService.Application.Dto;
using TradingPairService.Application.Interfaces.Repositories;
using TradingPairService.Application.Interfaces.Services;
using TradingPairService.Domain.Entities;

namespace TradingPairService.Application.Services
{
    public class TradingPairService : ITradingPairService
    {
        private const string SystemActor = "TradingPairService";

        private readonly ITradingPairRepository _repository;

        public TradingPairService(ITradingPairRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TradingPairResponse>> GetAll()
        {
            var pairs = await _repository.GetAll();
            return pairs.Select(Map).ToList();
        }

        public async Task<TradingPairResponse?> GetBySymbol(string symbol)
        {
            var pair = await _repository.GetBySymbol(NormalizeSymbol(symbol));
            return pair is null ? null : Map(pair);
        }

        public async Task<TradingPairResponse> Create(CreateTradingPairRequest request)
        {
            var baseAsset = request.BaseAsset.ToUpperInvariant().Trim();
            var quoteAsset = request.QuoteAsset.ToUpperInvariant().Trim();
            var symbol = $"{baseAsset}{quoteAsset}";

            if (baseAsset == quoteAsset)
                throw new InvalidOperationException("Base asset and quote asset cannot be the same.");

            if (await _repository.Exists(symbol))
                throw new InvalidOperationException("Trading pair already exists.");

            var now = DateTimeOffset.UtcNow;
            var pair = new TradingPair
            {
                Id = Guid.NewGuid(),
                Symbol = symbol,
                BaseAsset = baseAsset,
                QuoteAsset = quoteAsset,
                MinOrderQuantity = request.MinOrderQuantity,
                MinOrderValue = request.MinOrderValue,
                PricePrecision = request.PricePrecision,
                QuantityPrecision = request.QuantityPrecision,
                IsActive = true,
                CreatedDate = now,
                CreatedBy = SystemActor,
                ModifiedDate = now,
                ModifiedBy = SystemActor
            };

            await _repository.Add(pair);

            return Map(pair);
        }

        public async Task Activate(string symbol)
        {
            var pair = await _repository.GetBySymbol(NormalizeSymbol(symbol));

            if (pair is null)
                throw new InvalidOperationException("Trading pair not found.");

            pair.IsActive = true;
            pair.ModifiedDate = DateTimeOffset.UtcNow;
            pair.ModifiedBy = SystemActor;

            await _repository.Update(pair);
        }

        public async Task Deactivate(string symbol)
        {
            var pair = await _repository.GetBySymbol(NormalizeSymbol(symbol));

            if (pair is null)
                throw new InvalidOperationException("Trading pair not found.");

            pair.IsActive = false;
            pair.ModifiedDate = DateTimeOffset.UtcNow;
            pair.ModifiedBy = SystemActor;

            await _repository.Update(pair);
        }

        private static string NormalizeSymbol(string symbol)
        {
            return symbol.ToUpperInvariant().Trim();
        }

        private static TradingPairResponse Map(TradingPair pair)
        {
            return new TradingPairResponse
            {
                Symbol = pair.Symbol,
                BaseAsset = pair.BaseAsset,
                QuoteAsset = pair.QuoteAsset,
                MinOrderQuantity = pair.MinOrderQuantity,
                MinOrderValue = pair.MinOrderValue,
                PricePrecision = pair.PricePrecision,
                QuantityPrecision = pair.QuantityPrecision,
                IsActive = pair.IsActive
            };
        }
    }
}
