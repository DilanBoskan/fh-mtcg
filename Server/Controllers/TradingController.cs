using Application.Cards;
using Application.Trading;
using Domain;
using Server.Attributes;
using Server.Common;
using Server.Helpers;
using Server.Interfaces;
using System.Text.RegularExpressions;

namespace Server.Controllers;

public class TradingController : ControllerBase, ITradingController {
    private readonly ITradingService _tradingService;
    private readonly ICardsService _cardsService;

    public TradingController(ITradingService tradingService, ICardsService cardsService) {
        _tradingService = tradingService;
        _cardsService = cardsService;
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> GetAsync(HttpRequest request) {
        var trades = await _tradingService.GetAsync();

        if (!trades.Any())
            return CreateNoContentResponse();

        return CreateOKResponse(trades);
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> CreateAsync(HttpRequest request) {
        if (!Serializer.TryDeserialize<Trade>(request.Body, out var trade))
            return CreateBadRequestResponse();
        trade.OwnerUsername = request.Authorization!.Username;

        var canTrade = await _cardsService.CanTradeAsync(request.Authorization!.Username, trade.CardToTrade);
        if (!canTrade)
            return CreateForbiddenResponse();

        var trades = await _tradingService.GetAsync();
        if (trades.Any(t => t.Id == trade.Id))
            return CreateConflictResponse();

        await _tradingService.CreateAsync(trade);

        return CreateCreatedResponse();
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> DeleteAsync(HttpRequest request) {
        var match = Regex.Match(request.Url, @"tradings\/([\w-]+)");
        if (!match.Success)
            return CreateBadRequestResponse();
        var tradeId = Guid.Parse(match.Groups[1].Value);

        var trade = (await _tradingService.GetAsync()).FirstOrDefault(t => t.Id == tradeId);
        if (trade is null)
            return CreateNotFoundResponse();
        if (trade.OwnerUsername != request.Authorization!.Username)
            return CreateForbiddenResponse();

        await _tradingService.DeleteAsync(tradeId);

        return CreateOKResponse();
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> TradeAsync(HttpRequest request) {
        // Extract data
        var match = Regex.Match(request.Url, @"tradings\/([\w-]+)");
        if (!match.Success)
            return CreateBadRequestResponse();
        var tradeId = Guid.Parse(match.Groups[1].Value);

        var sendingCardId = Guid.Parse(request.Body.Trim('"'));

        // Retrieve card that will be sent
        var sendingCard = (await _cardsService.GetAsync(request.Authorization!.Username)).FirstOrDefault(c => c.Id == sendingCardId);
        if (sendingCard is null)
            return CreateForbiddenResponse();

        // Retrieve the trade
        var trade = (await _tradingService.GetAsync()).FirstOrDefault(t => t.Id == tradeId);
        if (trade is null)
            return CreateNotFoundResponse();

        // Validate that the trade is valid
        var tradingWithSelf = trade.OwnerUsername == request.Authorization!.Username;
        var canTrade = await _cardsService.CanTradeAsync(request.Authorization!.Username, sendingCardId);
        var requirementsMet = CanTrade(trade, sendingCard);
        if (tradingWithSelf || !requirementsMet || !canTrade)
            return CreateForbiddenResponse();

        await _tradingService.TradeAsync(tradeId, request.Authorization!.Username, sendingCardId);

        return CreateOKResponse();
    }

    private bool CanTrade(Trade trade, Card offer) {
        return offer.CardType == trade.Type &&
            offer.Damage >= trade.MinimumDamage;
    }
}
