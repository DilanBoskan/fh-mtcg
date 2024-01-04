using Domain;

namespace Application.Cards;

public interface ICardsService {
    Task<bool> ExistsAsync(Guid cardId);
    Task<bool> CanTradeAsync(string username, Guid cardId);
    Task<IReadOnlyList<Card>> GetAsync(string username);
    Task<IReadOnlyList<Card>> GetDeckAsync(string username);
    Task SetDeckAsync(string username, IReadOnlyList<Guid> cards);
}