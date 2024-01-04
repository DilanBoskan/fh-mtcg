using Domain;
using Domain.Repositories;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Sql;
public class CardsRepository : RepositoryBase, ICardsRepository {
    public CardsRepository(ConnectionInfo connectionInfo) : base(connectionInfo.ConnectionString) { }

    public async Task<IReadOnlyList<Card>> GetAsync(string username) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string query = """
            SELECT c.Id, c.Name, c.Damage, c.CardType, c.ElementType
            FROM Cards c
            JOIN UserCards uc ON c.Id = uc.CardId
            JOIN Users u ON uc.UserId = u.Id
            WHERE u.Username = @Username;
            """;

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        var cards = new List<Card>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cards.Add(new Card(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetFloat(2),
                Enum.Parse<CardType>(reader.GetString(3)),
                Enum.Parse<ElementType>(reader.GetString(4))
            ));
        }

        return cards;
    }

    public async Task<Card?> GetAsync(Guid cardId) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string query = """
            SELECT Id, Name, Damage, CardType, ElementType
            FROM Cards
            WHERE Id = @CardId;
            """;

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@CardId", cardId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Card(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetFloat(2),
                Enum.Parse<CardType>(reader.GetString(3)),
                Enum.Parse<ElementType>(reader.GetString(4))
            );
        }

        return null;
    }

    public async Task<IReadOnlyList<Card>> GetDeckAsync(string username) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string query = """
            SELECT c.Id, c.Name, c.Damage, c.CardType, c.ElementType
            FROM Cards c
            JOIN UserDecks ud ON c.Id = ud.CardId
            JOIN Users u ON ud.UserId = u.Id
            WHERE u.Username = @Username;
            """;

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        var cards = new List<Card>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            cards.Add(new Card(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetFloat(2),
                Enum.Parse<CardType>(reader.GetString(3)),
                Enum.Parse<ElementType>(reader.GetString(4))
            ));
        }

        return cards;
    }

    public async Task UpdateAsync(string username, IReadOnlyList<Card> cards) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM UserCards WHERE UserId = (SELECT Id FROM Users WHERE Username = @Username);";
        command.Parameters.AddWithValue("@Username", username);
        await command.ExecuteNonQueryAsync();

        command.CommandText = """
            INSERT INTO UserCards (UserId, CardId)
            VALUES ((SELECT Id FROM Users WHERE Username = @Username), @CardId)
            """;

        foreach (var card in cards) {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@CardId", card.Id);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task UpdateDeckAsync(string username, IReadOnlyList<Guid> cardIds) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM UserDecks WHERE UserId = (SELECT Id FROM Users WHERE Username = @Username);";
        command.Parameters.AddWithValue("@Username", username);
        await command.ExecuteNonQueryAsync();

        command.CommandText = """
            INSERT INTO UserDecks (UserId, CardId)
            VALUES ((SELECT Id FROM Users WHERE Username = @Username), @CardId)
            """;

        foreach (var cardId in cardIds) {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@CardId", cardId);
            await command.ExecuteNonQueryAsync();
        }
    }
}
