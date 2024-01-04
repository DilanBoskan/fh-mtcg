namespace Domain.Repositories;
public interface ITradingRepository {
    Task<IReadOnlyList<Trade>> GetAsync();
    Task<Trade?> GetAsync(Guid id);
    Task CreateAsync(Trade trade);
    Task DeleteAsync(Guid id);
}
