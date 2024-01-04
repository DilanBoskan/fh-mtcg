using Application.Cards;
using Application.Money;
using Application.Packages;
using Domain;
using Server.Attributes;
using Server.Common;
using Server.Helpers;
using Server.Interfaces;

namespace Server.Controllers;

public class PackagesController : ControllerBase, IPackagesController {
    private readonly IPackagesService _packagesService;
    private readonly ICardsService _cardsService;
    private readonly IMoneyService _moneyService;

    public PackagesController(IPackagesService packagesService, ICardsService cardsService, IMoneyService moneyService) {
        _packagesService = packagesService;
        _cardsService = cardsService;
        _moneyService = moneyService;
    }

    [RequiresAuthorization(UserRole.Admin)]
    public async Task<HttpResponse> CreateAsync(HttpRequest request) {
        if (!Serializer.TryDeserialize<IReadOnlyList<Card>>(request.Body, out var cards))
            return CreateBadRequestResponse();

        // Check if any card already exists
        foreach (var card in cards) {
            if (await _cardsService.ExistsAsync(card.Id))
                return CreateConflictResponse();
        }

        // Extract card properties from name
        cards = cards.Select(c => {
            var cardType = c.Name.ToLowerInvariant() switch {
                var str when str.Contains("spell") => CardType.Spell,
                _ => CardType.Monster
            };
            var elementType = c.Name.ToLowerInvariant() switch {
                var str when str.Contains("water") => ElementType.Water,
                var str when str.Contains("fire") => ElementType.Fire,
                _ => ElementType.Normal
            };

            return c with {
                CardType = cardType,
                ElementType = elementType,
            };
        }).ToList();

        await _packagesService.CreateAsync(cards);

        return CreateCreatedResponse();
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> AcquireAsync(HttpRequest request) {
        // Check if enough money
        var availableMoney = await _moneyService.GetMoneyAsync(request.Authorization!.Username);
        if (availableMoney < PackagesService.PACKAGE_ACQUIRE_COST)
            return CreateForbiddenResponse();

        // Check if there is a package to buy
        var availablePackages = await _packagesService.GetAsync();
        if (!availablePackages.Any())
            return CreateNotFoundResponse();

        var package = await _packagesService.AcquireAsync(request.Authorization!.Username);

        return CreateOKResponse(package.Cards);
    }
}
