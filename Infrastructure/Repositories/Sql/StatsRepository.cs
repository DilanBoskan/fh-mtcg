using Domain;
using Domain.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Sql;
public class StatsRepository : RepositoryBase, IStatsRepository {
    public StatsRepository(ConnectionInfo connectionInfo) : base(connectionInfo.ConnectionString) { }

    public async Task CreateAsync(Stats stats) {
        await using var connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        string commandText = """
            INSERT INTO Stats (UserId, Elo, Wins, Losses) 
            VALUES ((SELECT Id FROM Users WHERE Username = @Username), @Elo, @Wins, @Losses);
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Username", stats.Username);
        command.Parameters.AddWithValue("@Elo", stats.Elo);
        command.Parameters.AddWithValue("@Wins", stats.Wins);
        command.Parameters.AddWithValue("@Losses", stats.Losses);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Stats stats) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            UPDATE Stats
            SET Elo = @Elo, Wins = @Wins, Losses = @Losses
            WHERE UserId = (SELECT Id FROM Users WHERE Username = @Username);
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Username", stats.Username);
        command.Parameters.AddWithValue("@Elo", stats.Elo);
        command.Parameters.AddWithValue("@Wins", stats.Wins);
        command.Parameters.AddWithValue("@Losses", stats.Losses);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Stats?> GetAsync(string username) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            SELECT Elo, Wins, Losses
            FROM Stats
            WHERE UserId = (SELECT Id FROM Users WHERE Username = @Username);
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Stats(
                username,
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2)
            );
        }

        return null;
    }

    public async Task<IReadOnlyList<Stats>> GetAsync() {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = "SELECT (SELECT Username FROM Users WHERE Id = UserId), Elo, Wins, Losses FROM Stats;";

        await using var command = new NpgsqlCommand(commandText, connection);

        var statsList = new List<Stats>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            statsList.Add(new Stats(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3)
            ));
        }

        return statsList;
    }
}
