using Server.Attributes;
using Server.Common;
using Server.Exceptions;
using Server.Interfaces;
using System.Net;
using System.Text.RegularExpressions;

namespace Server.Server;

/// <summary>
/// Request handler and forwarder
/// </summary>
public class HttpRouter : IHttpRouter {
    private readonly IUsersController _userController;
    private readonly IPackagesController _packagesController;
    private readonly ICardsController _cardsController;
    private readonly IGameController _gameController;
    private readonly ITradingController _tradingController;

    public HttpRouter(IUsersController userController, IPackagesController packagesController, ICardsController cardsController, IGameController gameController, ITradingController tradingController) {
        _userController = userController;
        _packagesController = packagesController;
        _cardsController = cardsController;
        _gameController = gameController;
        _tradingController = tradingController;
    }

    public bool CanHandleHttpRequest(HttpRequest request) => GetHandler(request) is not null;

    public async Task<HttpResponse> HandleHttpRequestAsync(HttpRequest request) {
        HttpRequestHandler requestHandler = GetHandler(request) ?? throw new UnableToHandleHttpRequestException();

        if (Attribute.GetCustomAttribute(requestHandler.Method, typeof(RequiresAuthorizationAttribute)) is RequiresAuthorizationAttribute authorizationAttribute) {
            if (request.Authorization is null)
                return HttpResponse.Create(HttpStatusCode.Unauthorized);

            var requiredRole = authorizationAttribute.Role;

            // Check authorization
            bool authorized = (request.Authorization.Role & requiredRole) != 0;
            if (!authorized)
                return HttpResponse.Create(HttpStatusCode.Forbidden);
        }

        return await requestHandler(request);
    }

    private HttpRequestHandler? GetHandler(HttpRequest request) {
        return request switch {
            // Users
            { Method: "POST", Url: "/users" } => _userController.RegisterAsync, { Method: "GET" } when Regex.IsMatch(request.Url, @"/users/\w+") => _userController.GetAsync, { Method: "PUT" } when Regex.IsMatch(request.Url, @"/users/\w+") => _userController.UpdateAsync, { Method: "POST", Url: "/sessions" } => _userController.LoginAsync,
            // Packages
            { Method: "POST", Url: "/packages" } => _packagesController.CreateAsync, { Method: "POST", Url: "/transactions/packages" } => _packagesController.AcquireAsync,
            // Cards
            { Method: "GET", Url: "/cards" } => _cardsController.GetAsync, { Method: "GET" } when Regex.IsMatch(request.Url, @"/deck") => _cardsController.GetDeckAsync, { Method: "PUT", Url: "/deck" } => _cardsController.SetDeckAsync,
            // Game
            { Method: "GET", Url: "/stats" } => _gameController.GetStatsAsync, { Method: "GET", Url: "/scoreboard" } => _gameController.GetScoreboardAsync, { Method: "POST", Url: "/battles" } => _gameController.EnterBattleAsync,
            // Trading
            { Method: "GET", Url: "/tradings" } => _tradingController.GetAsync, { Method: "POST", Url: "/tradings" } => _tradingController.CreateAsync, { Method: "DELETE" } when Regex.IsMatch(request.Url, @"/tradings/\w+") => _tradingController.DeleteAsync, { Method: "POST" } when Regex.IsMatch(request.Url, @"/tradings/\w+") => _tradingController.TradeAsync,

            _ => null,
        };
    }
}
