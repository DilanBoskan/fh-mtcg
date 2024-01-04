using Domain.Repositories;

namespace Application.Money;
public class MoneyService : IMoneyService {
    private readonly IMoneyRepository _moneyRepository;

    public MoneyService(IMoneyRepository moneyRepository) {
        _moneyRepository = moneyRepository;
    }
    public async Task<int?> GetMoneyAsync(string username) => await _moneyRepository.GetMoneyAsync(username);
}
