using Domain;
using Domain.Repositories;
using System.Security.Cryptography;

namespace Application.Users;


public class UsersService : IUsersService {
    public const int HASH_SIZE_LENGTH = 20;
    public const int PASSWORD_SALT_LENGTH = 5;
    public const int STARTING_MONEY = 20;

    private readonly IUsersRepository _userRepository;
    private readonly IMoneyRepository _moneyRepository;
    private readonly IStatsRepository _statsRepository;

    public UsersService(IUsersRepository userRepository, IMoneyRepository moneyRepository, IStatsRepository statsRepository) {
        _userRepository = userRepository;
        _moneyRepository = moneyRepository;
        _statsRepository = statsRepository;
    }

    public async Task<bool> ExistsAsync(string username) => await _userRepository.GetAsync(username) is not null;
    public async Task<UserInfo> GetAsync(string username) {
        var user = await _userRepository.GetAsync(username);
        ArgumentNullException.ThrowIfNull(user);

        return new UserInfo(user.Name, user.Bio, user.Image);
    }
    public async Task UpdateAsync(string username, UserInfo userInfo) => await _userRepository.UpdateAsync(username, userInfo.Name, userInfo.Bio, userInfo.Image);
    public async Task RegisterAsync(string username, string password) {
        var hashedPassword = HashPassword(password, RandomNumberGenerator.GetBytes(PASSWORD_SALT_LENGTH));

        var user = new User(username, hashedPassword, username, string.Empty, string.Empty);

        await _userRepository.CreateAsync(user);
        await _moneyRepository.SetMoneyAsync(username, STARTING_MONEY);
        await _statsRepository.CreateAsync(Stats.Empty(username));
    }

    public async Task<LoginResult> LoginAsync(string username, string password) {
        var user = await _userRepository.GetAsync(username);
        ArgumentNullException.ThrowIfNull(user);

        byte[] hashedPassword = Convert.FromBase64String(user.Password);
        /* Get the salt */
        byte[] salt = new byte[PASSWORD_SALT_LENGTH];
        Array.Copy(hashedPassword, 0, salt, 0, PASSWORD_SALT_LENGTH);

        var hash = HashPassword(password, salt);
        if (hash != user.Password)
            return new LoginResult(false, null);

        return new LoginResult(true, $"{username}-mtcgToken");
    }

    private string HashPassword(string password, byte[] salt) {
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, HASH_SIZE_LENGTH);

        byte[] hashBytes = new byte[PASSWORD_SALT_LENGTH + HASH_SIZE_LENGTH];
        Array.Copy(salt, 0, hashBytes, 0, PASSWORD_SALT_LENGTH);
        Array.Copy(hash, 0, hashBytes, PASSWORD_SALT_LENGTH, HASH_SIZE_LENGTH);

        return Convert.ToBase64String(hashBytes);
    }
}
