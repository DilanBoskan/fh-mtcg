namespace Domain.Repositories;

public interface IMoneyRepository {
    Task<int?> GetMoneyAsync(string username);
    Task SetMoneyAsync(string username, int money);
}