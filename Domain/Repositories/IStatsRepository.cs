namespace Domain.Repositories;
public interface IStatsRepository {
    Task CreateAsync(Stats stats);
    Task UpdateAsync(Stats stats);
    Task<Stats?> GetAsync(string username);
    Task<IReadOnlyList<Stats>> GetAsync();
}
