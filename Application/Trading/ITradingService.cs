using Domain;

namespace Application.Trading;

public interface ITradingService {
    Task CreateAsync(Trade trade);
    Task DeleteAsync(Guid id);
    Task<IReadOnlyList<Trade>> GetAsync();
    Task TradeAsync(Guid tradeId, string username, Guid cardId);
}