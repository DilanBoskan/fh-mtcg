namespace Domain.Repositories;

public interface IUsersRepository {
    Task CreateAsync(User user);
    Task DeleteAsync(string username);
    Task<User?> GetAsync(string username);
    Task UpdateAsync(string username, string name, string bio, string image);
}