namespace TradingService.Domain.Entities
{
    public enum OrderStatus
    {
        None,
        Pending,
        Filled,
        PartiallyFilled,
        Canceled,
        Rejected
    }
}
