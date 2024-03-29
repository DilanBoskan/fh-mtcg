﻿using Domain;
using Domain.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Sql;
public class TradingRepository : RepositoryBase, ITradingRepository {
    public TradingRepository(ConnectionInfo connectionInfo) : base(connectionInfo.ConnectionString) { }

    public async Task<IReadOnlyList<Trade>> GetAsync() {
        var trades = new List<Trade>();
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = "SELECT Id, (SELECT Username FROM Users WHERE Id = UserId), CardToTrade, Type, MinimumDamage FROM Trades;";

        await using var command = new NpgsqlCommand(commandText, connection);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            trades.Add(new Trade(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetGuid(2),
                Enum.Parse<CardType>(reader.GetString(3)),
                reader.GetInt32(4)
            ));
        }

        return trades;
    }

    public async Task<Trade?> GetAsync(Guid id) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            SELECT Id, (SELECT Username FROM Users WHERE Id = UserId), CardToTrade, Type, MinimumDamage
            FROM Trades
            WHERE Id = @Id;
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Trade(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetGuid(2),
                Enum.Parse<CardType>(reader.GetString(3)),
                reader.GetInt32(4)
            );
        }

        return null;
    }

    public async Task CreateAsync(Trade trade) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            INSERT INTO Trades (Id, UserId, CardToTrade, Type, MinimumDamage) 
            VALUES (@Id, (SELECT Id FROM Users WHERE Username = @Username), @CardToTrade, @Type::CardType, @MinimumDamage);
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", trade.Id);
        command.Parameters.AddWithValue("@Username", trade.OwnerUsername);
        command.Parameters.AddWithValue("@CardToTrade", trade.CardToTrade);
        command.Parameters.AddWithValue("@Type", trade.Type.ToString());
        command.Parameters.AddWithValue("@MinimumDamage", trade.MinimumDamage);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(Guid id) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            DELETE FROM Trades 
            WHERE Id = @Id;
            """;

        await using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync();
    }
}
