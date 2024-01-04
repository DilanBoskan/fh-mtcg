using Domain.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Sql;
public class MoneyRepository : RepositoryBase, IMoneyRepository {
    public MoneyRepository(ConnectionInfo connectionInfo) : base(connectionInfo.ConnectionString) { }

    public async Task<int?> GetMoneyAsync(string username) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            SELECT Money
            FROM UserMoney
            JOIN Users u ON Username = @Username
            WHERE UserId = u.Id;
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync()) {
            return reader.GetInt32(0);
        }

        return null;
    }

    public async Task SetMoneyAsync(string username, int money) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            INSERT INTO UserMoney (UserId, Money)
            VALUES ((SELECT Id FROM Users WHERE Username = @Username), @Money)
            ON CONFLICT (UserId)
            DO UPDATE SET Money = EXCLUDED.Money;
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@Money", money);

        await command.ExecuteNonQueryAsync();
    }
}
