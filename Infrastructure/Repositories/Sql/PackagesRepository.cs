using Domain;
using Domain.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Infrastructure.Repositories.Sql;
public class PackagesRepository : RepositoryBase, IPackagesRepository {
    public PackagesRepository(ConnectionInfo connectionInfo) : base(connectionInfo.ConnectionString) { }

    public async Task<IReadOnlyList<Package>> GetAsync() {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = @"
            SELECT p.Id, c.Id, c.Name, c.Damage, c.CardType, c.ElementType
            FROM Packages p
            JOIN PackageCards pc ON p.Id = pc.PackageId
            JOIN Cards c ON pc.CardId = c.Id;
        ";

        await using var command = new NpgsqlCommand(commandText, connection);

        var packages = new List<Package>();
        await using var reader = await command.ExecuteReaderAsync();

        var packageMap = new Dictionary<Guid, List<Card>>();
        while (await reader.ReadAsync()) {
            var packageId = reader.GetGuid(0);
            if (!packageMap.ContainsKey(packageId)) {
                packageMap[packageId] = new List<Card>();
            }

            var card = new Card(
                reader.GetGuid(1),
                reader.GetString(2),
                reader.GetFloat(3),
                Enum.Parse<CardType>(reader.GetString(4)),
                Enum.Parse<ElementType>(reader.GetString(5))
            );

            packageMap[packageId].Add(card);
        }

        foreach (var entry in packageMap) {
            packages.Add(new Package(entry.Key, entry.Value.AsReadOnly()));
        }

        return packages.AsReadOnly();
    }

    public async Task CreateAsync(Package package) {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        // Insert package
        command.CommandText = """
            INSERT INTO Packages (Id) 
            VALUES (@Id);
            """;
        command.Parameters.AddWithValue("@Id", package.Id);
        await command.ExecuteNonQueryAsync();

        // Insert cards
        command.CommandText = """
            INSERT INTO Cards (Id, Name, Damage, CardType, ElementType)
            VALUES (@Id, @Name, @Damage, @CardType::CardType, @ElementType::ElementType);
            """;
        foreach (var card in package.Cards) {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Id", card.Id);
            command.Parameters.AddWithValue("@Name", card.Name);
            command.Parameters.AddWithValue("@Damage", card.Damage);
            command.Parameters.AddWithValue("@CardType", card.CardType.ToString());
            command.Parameters.AddWithValue("@ElementType", card.ElementType.ToString());
            await command.ExecuteNonQueryAsync();
        }

        // Insert package cards
        command.CommandText = """
            INSERT INTO PackageCards (PackageId, CardId) 
            VALUES (@PackageId, @CardId);
            """;
        foreach (var card in package.Cards) {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@PackageId", package.Id);
            command.Parameters.AddWithValue("@CardId", card.Id);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task DeleteAsync(Guid id) {
        using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        string commandText = """
            DELETE FROM Packages 
            WHERE Id = @Id;
            """;

        using var command = new NpgsqlCommand(commandText, connection);
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync();
    }
}
