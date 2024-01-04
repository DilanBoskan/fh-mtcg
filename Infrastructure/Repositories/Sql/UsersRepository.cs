using Domain;
using Domain.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Sql;
public class UsersRepository : RepositoryBase, IUsersRepository {
    public UsersRepository(ConnectionInfo connectionInfo) : base(connectionInfo.ConnectionString) { }

    public async Task CreateAsync(User user) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            INSERT INTO Users (Id, Username, Password, Name, Bio, Image) 
            VALUES (@Id, @Username, @Password, @Name, @Bio, @Image);
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", Guid.NewGuid());
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Password", user.Password);
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Bio", user.Bio);
        command.Parameters.AddWithValue("@Image", user.Image);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(string username) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            DELETE FROM Users 
            WHERE Username = @Username;
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Username", username);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetAsync(string username) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            SELECT Username, Password, Name, Bio, Image 
            FROM Users 
            WHERE Username = @Username;
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4)
            );
        }

        return null;
    }

    public async Task UpdateAsync(string username, string name, string bio, string image) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            UPDATE Users 
            SET Name = @Name, Bio = @Bio, Image = @Image 
            WHERE Username = @Username;
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Bio", bio);
        command.Parameters.AddWithValue("@Image", image);

        await command.ExecuteNonQueryAsync();
    }
}
