using Domain;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Sql;
public class DatabaseConfiguration : RepositoryBase {
    public DatabaseConfiguration(ConnectionInfo connectionInfo) : base(connectionInfo.ConnectionString) { }

    public async Task InitializeAsync() {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        // Create CardType and ElementType Enums
        command.CommandText = """
            DO $$ BEGIN
                CREATE TYPE CardType AS ENUM ('Monster', 'Spell');
            EXCEPTION
                WHEN duplicate_object THEN null;
            END $$;
            DO $$ BEGIN
                CREATE TYPE ElementType AS ENUM ('Water', 'Fire', 'Normal');
            EXCEPTION
                WHEN duplicate_object THEN null;
            END $$;
            """;
        await command.ExecuteNonQueryAsync();


        // Cards
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Cards (
                Id UUID PRIMARY KEY,
                Name TEXT NOT NULL,
                Damage REAL NOT NULL,
                CardType CardType NOT NULL,
                ElementType ElementType NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync();


        // Users
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Users (
                Id UUID PRIMARY KEY,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Name TEXT NOT NULL,
                Bio TEXT NOT NULL,
                Image TEXT NOT NULL
            );

            -- Relationships
            CREATE TABLE IF NOT EXISTS UserCards (
                UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
                CardId UUID PRIMARY KEY REFERENCES Cards(Id) ON DELETE CASCADE
            );
            CREATE TABLE IF NOT EXISTS UserDecks (
                UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
                CardId UUID PRIMARY KEY REFERENCES Cards(Id) ON DELETE CASCADE
            );
            CREATE TABLE IF NOT EXISTS UserMoney (
                UserId UUID PRIMARY KEY REFERENCES Users(Id) ON DELETE CASCADE,
                Money INT
            );
            """;
        await command.ExecuteNonQueryAsync();

        // Trades
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Trades (
                Id UUID PRIMARY KEY,
                UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
                CardToTrade UUID NOT NULL REFERENCES Cards(Id) ON DELETE CASCADE,
                Type CardType NOT NULL,
                MinimumDamage INT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync();


        // Packages
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Packages (
                Id UUID PRIMARY KEY
            );

            -- Relationships
            CREATE TABLE IF NOT EXISTS PackageCards (
                PackageId UUID,
                CardId UUID PRIMARY KEY REFERENCES Cards(Id) ON DELETE CASCADE
            );
            """;
        await command.ExecuteNonQueryAsync();


        // Stats
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Stats (
                UserId UUID PRIMARY KEY REFERENCES Users(Id) ON DELETE CASCADE,
                Elo INT NOT NULL,
                Wins INT NOT NULL,
                Losses INT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task ClearDatabaseAsync() {
        await using var connection = _dataSource.CreateConnection();;
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        // Clear all the tables
        command.CommandText = """
            TRUNCATE TABLE Cards CASCADE;

            TRUNCATE TABLE Trades CASCADE;

            TRUNCATE TABLE Packages CASCADE;
            TRUNCATE TABLE PackageCards CASCADE;

            TRUNCATE TABLE Users CASCADE;
            TRUNCATE TABLE UserCards CASCADE;
            TRUNCATE TABLE UserDecks CASCADE;
            TRUNCATE TABLE UserMoney CASCADE;
            
            TRUNCATE TABLE Stats CASCADE;
            """;
        await command.ExecuteNonQueryAsync();
    }
}