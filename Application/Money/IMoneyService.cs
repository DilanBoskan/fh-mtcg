namespace Application.Money;

public interface IMoneyService {
    Task<int?> GetMoneyAsync(string username);
}
