using Domain;
using Domain.Repositories;

namespace Infrastructure.Repositories.InMemory;

public class InMemoryCardsRepository : ICardsRepository
{
    public Task<Card?> GetAsync(Guid cardId)
    {
        var card = _cards.Values.FirstOrDefault(c => c.Any(userC => userC.Id == cardId))?.First(userC => userC.Id == cardId);
        return Task.FromResult(card);
    }
    public Task<IReadOnlyList<Card>> GetAsync(string username)
    {
        if (!_cards.TryGetValue(username, out var cards))
            return Task.FromResult((IReadOnlyList<Card>)Array.Empty<Card>());

        return Task.FromResult(cards);
    }
    public Task UpdateAsync(string username, IReadOnlyList<Card> cards)
    {
        _cards[username] = cards;
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Card>> GetDeckAsync(string username)
    {
        if (!_decks.TryGetValue(username, out var deckCards))
            return Array.Empty<Card>();

        var cards = await GetAsync(username);

        return cards.Where(c => deckCards.Contains(c.Id)).ToList();
    }

    public Task UpdateDeckAsync(string username, IReadOnlyList<Guid> cards)
    {
        _decks[username] = cards;
        return Task.CompletedTask;
    }

    private Dictionary<string, IReadOnlyList<Card>> _cards = new();
    private Dictionary<string, IReadOnlyList<Guid>> _decks = new();
}
