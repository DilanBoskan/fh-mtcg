using Domain.Repositories;

namespace Infrastructure.Repositories.InMemory;
public class InMemoryMoneyRepository : IMoneyRepository
{
    public Task<int?> GetMoneyAsync(string username)
    {
        if (!_money.TryGetValue(username, out var value))
            return Task.FromResult<int?>(null);

        return Task.FromResult<int?>(value);
    }

    public Task SetMoneyAsync(string username, int money)
    {
        _money[username] = money;
        return Task.CompletedTask;
    }

    private Dictionary<string, int> _money = new();
}
