namespace Domain.Repositories;
public interface ICardsRepository {
    Task<IReadOnlyList<Card>> GetAsync(string username);
    Task<Card?> GetAsync(Guid cardId);
    Task UpdateAsync(string username, IReadOnlyList<Card> cards);
    Task<IReadOnlyList<Card>> GetDeckAsync(string username);
    Task UpdateDeckAsync(string username, IReadOnlyList<Guid> cardIds);
}
