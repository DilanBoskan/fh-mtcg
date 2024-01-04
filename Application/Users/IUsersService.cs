namespace Application.Users;

public record UserInfo(string Name, string Bio, string Image);
public record LoginResult(bool Success, string? AccessToken);

public interface IUsersService {
    Task RegisterAsync(string username, string password);
    Task<UserInfo> GetAsync(string username);
    Task UpdateAsync(string username, UserInfo userInfo);
    Task<LoginResult> LoginAsync(string username, string password);

    Task<bool> ExistsAsync(string username);
}