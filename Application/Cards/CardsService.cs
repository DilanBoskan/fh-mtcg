using Domain;
using Domain.Repositories;

namespace Application.Cards;
public class CardsService : ICardsService {
    private readonly ICardsRepository _cardsRepository;

    public CardsService(ICardsRepository cardsRepository) {
        _cardsRepository = cardsRepository;
    }

    public async Task<bool> ExistsAsync(Guid cardId) => await _cardsRepository.GetAsync(cardId) is not null;
    public async Task<bool> CanTradeAsync(string username, Guid cardId) {
        var cards = await GetAsync(username);
        if (!cards.Any(c => c.Id == cardId))
            return false;
        var deckCards = await GetDeckAsync(username);
        if (deckCards.Any(c => c.Id == cardId))
            return false;

        return true;
    }
    public async Task<IReadOnlyList<Card>> GetAsync(string username) => await _cardsRepository.GetAsync(username);
    public async Task<IReadOnlyList<Card>> GetDeckAsync(string username) => await _cardsRepository.GetDeckAsync(username);
    public async Task SetDeckAsync(string username, IReadOnlyList<Guid> cards) {
        if (cards.Count != 4)
            throw new ArgumentException("Cards count must be four!");

        await _cardsRepository.UpdateDeckAsync(username, cards);
    }
}