using Application.Cards;
using Server.Attributes;
using Server.Common;
using Server.Helpers;
using Server.Interfaces;

namespace Server.Controllers;

public class CardsController : ControllerBase, ICardsController {
    private readonly ICardsService _cardService;

    public CardsController(ICardsService cardService) {
        _cardService = cardService;
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> GetAsync(HttpRequest request) {
        var cards = await _cardService.GetAsync(request.Authorization!.Username);

        if (!cards.Any())
            return CreateNoContentResponse();

        return CreateOKResponse(cards);
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> GetDeckAsync(HttpRequest request) {
        var cards = await _cardService.GetDeckAsync(request.Authorization!.Username);

        if (!cards.Any())
            return CreateNoContentResponse();

        var format = request.QueryParams.TryGetValue("format", out var value) ? value : "json";


        switch (format) {
            case "json":
                return CreateOKResponse(cards);
            case "plain":
                var plainText = $"The deck consists of the following cards:\n\n{string.Join(",\n", cards.Select(c => $"({c.Id}): {c.Name} with {c.Damage} damage"))}";
                return CreateOKResponse(plainText, "text/plain");
            default:
                throw new ArgumentException("Invalid format type!");
        }
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> SetDeckAsync(HttpRequest request) {
        if (!Serializer.TryDeserialize<IReadOnlyList<Guid>>(request.Body, out var values))
            return CreateBadRequestResponse();

        var cards = (await _cardService.GetAsync(request.Authorization!.Username)).Select(c => c.Id);
        if (cards.Intersect(values).Count() != 4)
            return CreateForbiddenResponse();

        await _cardService.SetDeckAsync(request.Authorization.Username, values);

        return CreateOKResponse();
    }
}
