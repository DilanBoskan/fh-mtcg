using Domain;
using Domain.Repositories;

namespace Infrastructure.Repositories.InMemory;
public class InMemoryTradingRepository : ITradingRepository
{
    public Task<IReadOnlyList<Trade>> GetAsync()
    {
        return Task.FromResult((IReadOnlyList<Trade>)_trades);
    }
    public Task<Trade?> GetAsync(Guid id)
    {
        return Task.FromResult(_trades.FirstOrDefault(t => t.Id == id));
    }
    public Task CreateAsync(Trade trade)
    {
        _trades.Add(trade);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _trades.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    private List<Trade> _trades = new();
}
