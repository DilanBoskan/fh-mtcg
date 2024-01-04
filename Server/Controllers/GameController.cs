using Application.Game;
using Server.Attributes;
using Server.Common;
using Server.Interfaces;

namespace Server.Controllers;
public class GameController : ControllerBase, IGameController {
    private readonly IGameService _gameService;

    public GameController(IGameService gameService) {
        _gameService = gameService;
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> GetStatsAsync(HttpRequest request) {
        var stats = await _gameService.GetStatsAsync(request.Authorization!.Username);

        return CreateOKResponse(stats);
    }
    [RequiresAuthorization]
    public async Task<HttpResponse> GetScoreboardAsync(HttpRequest request) {
        var scoreboard = await _gameService.GetScoreboardAsync();

        return CreateOKResponse(scoreboard);
    }
    [RequiresAuthorization]
    public async Task<HttpResponse> EnterBattleAsync(HttpRequest request) {
        var battleResult = await _gameService.EnterBattleAsync(request.Authorization!.Username);

        var response = $"Winner: {battleResult.WinnerUsername}\r\nLog:\r\n{string.Join("\r\n", battleResult.Log)}";

        return CreateOKResponse(response, "text/plain");
    }
}
