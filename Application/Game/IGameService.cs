using Domain;

namespace Application.Game;
public interface IGameService {
    Task<BattleResult> EnterBattleAsync(string username);
    Task<IReadOnlyList<Stats>> GetScoreboardAsync();
    Task<Stats> GetStatsAsync(string username);
}