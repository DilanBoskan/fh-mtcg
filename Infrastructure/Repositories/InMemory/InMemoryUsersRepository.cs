using Domain;
using Domain.Repositories;

namespace Infrastructure.Repositories.InMemory;

public class InMemoryUsersRepository : IUsersRepository
{
    public Task CreateAsync(User user)
    {
        _users.Add(user);

        return Task.CompletedTask;
    }
    public Task<User?> GetAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);

        return Task.FromResult(user);
    }
    public async Task UpdateAsync(string username, string name, string bio, string image)
    {
        var user = await GetAsync(username);
        ArgumentNullException.ThrowIfNull(user);

        await DeleteAsync(username);
        await CreateAsync(user with
        {
            Name = name,
            Bio = bio,
            Image = image
        });
    }
    public Task DeleteAsync(string username)
    {
        _users.RemoveAll(u => u.Username == username);

        return Task.CompletedTask;
    }

    private List<User> _users = new();
}
