using Domain;
using Domain.Repositories;

namespace Application.Trading;

public class TradingService : ITradingService {
    private readonly ITradingRepository _tradingRepository;
    private readonly ICardsRepository _cardsRepository;

    public TradingService(ITradingRepository tradingRepository, ICardsRepository cardsRepository) {
        _tradingRepository = tradingRepository;
        _cardsRepository = cardsRepository;
    }

    public async Task<IReadOnlyList<Trade>> GetAsync() => await _tradingRepository.GetAsync();
    public async Task CreateAsync(Trade trade) => await _tradingRepository.CreateAsync(trade);
    public async Task DeleteAsync(Guid id) => await _tradingRepository.DeleteAsync(id);
    public async Task TradeAsync(Guid tradeId, string username, Guid cardId) {
        var trade = await _tradingRepository.GetAsync(tradeId);
        ArgumentNullException.ThrowIfNull(trade);
        var receiverCard = await _cardsRepository.GetAsync(trade.CardToTrade);
        var senderCard = await _cardsRepository.GetAsync(cardId);
        ArgumentNullException.ThrowIfNull(receiverCard);
        ArgumentNullException.ThrowIfNull(senderCard);

        // Update receiver cards
        var receiverCards = (await _cardsRepository.GetAsync(trade.OwnerUsername)).ToList();
        receiverCards.RemoveAll(c => c.Id == receiverCard.Id);
        receiverCards.Add(senderCard);
        await _cardsRepository.UpdateAsync(trade.OwnerUsername, receiverCards);

        // Update retriever cards
        var senderCards = (await _cardsRepository.GetAsync(username)).ToList();
        senderCards.RemoveAll(c => c.Id == senderCard.Id);
        senderCards.Add(receiverCard);
        await _cardsRepository.UpdateAsync(username, senderCards);


        await DeleteAsync(tradeId);
    }
}
