using Server.Common;

namespace Server.Interfaces;
public interface IGameController {
    Task<HttpResponse> EnterBattleAsync(HttpRequest request);
    Task<HttpResponse> GetScoreboardAsync(HttpRequest request);
    Task<HttpResponse> GetStatsAsync(HttpRequest request);
}