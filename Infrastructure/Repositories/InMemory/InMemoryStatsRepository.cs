using Domain;
using Domain.Repositories;

namespace Infrastructure.Repositories.InMemory;
public class InMemoryStatsRepository : IStatsRepository
{
    public Task<Stats?> GetAsync(string username)
    {
        var stats = _stats.FirstOrDefault(x => x.Username == username);

        return Task.FromResult(stats);
    }

    public Task<IReadOnlyList<Stats>> GetAsync()
    {
        return Task.FromResult((IReadOnlyList<Stats>)_stats);
    }

    public Task CreateAsync(Stats stats) => UpdateAsync(stats);
    public Task UpdateAsync(Stats stats)
    {
        _stats.RemoveAll(s => s.Username == stats.Username);
        _stats.Add(stats);
        return Task.CompletedTask;
    }

    private List<Stats> _stats = new();
}
