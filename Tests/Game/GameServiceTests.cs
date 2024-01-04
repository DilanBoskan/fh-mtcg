using Application.Cards;
using Application.Game;
using Application.Packages;
using Application.Users;
using Domain;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;
using Tests.Extensions;

namespace Tests.Game;

public class StudRandomNumberGenerator : IRandomNumberGenerator {
    internal void _SetOrder(IReadOnlyList<int> values) {
        _values = values;
        _curIdx = 0;
    }

    public int Next(int max) {
        ArgumentNullException.ThrowIfNull(_values);
        if (_curIdx > _values.Count - 1) throw new ArgumentOutOfRangeException("Ran out of values for random number generator!");
        
        return _values[_curIdx++];
    }

    private IReadOnlyList<int>? _values;
    private int _curIdx = 0;
}

public class StudCardsRepository : ICardsRepository {
    private Dictionary<string, IReadOnlyList<Card>> _decks = new();
    internal void _SetDeck(string username, IReadOnlyList<Card> deck) {
        _decks[username] = deck;
    }
    public Task<IReadOnlyList<Card>> GetDeckAsync(string username) => Task.FromResult(_decks[username]);

    public Task<IReadOnlyList<Card>> GetAsync(string username) => throw new NotImplementedException();
    public Task<Card?> GetAsync(Guid cardId) => throw new NotImplementedException();
    public Task UpdateAsync(string username, IReadOnlyList<Card> cards) => throw new NotImplementedException();
    public Task UpdateDeckAsync(string username, IReadOnlyList<Guid> cardIds) => throw new NotImplementedException();
}

public class SimulatedGamesData : TheoryData<IReadOnlyList<Card>, IReadOnlyList<Card>, IReadOnlyList<int>, int> {
    public SimulatedGamesData() {
        AddGame1();
        AddGame2();
        AddGame3();
        AddDrawnGames();
    }

    public void AddGame1() {
        // Clear first winner
        var firstUserCards = new List<Card>() {
            new Card(Guid.NewGuid(), string.Empty, 100, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 100, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 100, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 100, CardType.Monster, ElementType.Normal),
        };
        var secondUserCards = new List<Card>() {
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
        };
        var randomNumberOrders = new int[] {
            0, 0,
            0, 0,
            0, 0,
            0, 0,
        };

        Add(firstUserCards, secondUserCards, randomNumberOrders, 1);
    }

    public void AddGame2() {
        // Also first winner becuase random numbers are just 0
        var firstUserCards = new List<Card>() {
            new Card(Guid.NewGuid(), string.Empty, 100, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 5, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 5, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 5, CardType.Monster, ElementType.Normal),
        };
        var secondUserCards = new List<Card>() {
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
        };
        var randomNumberOrders = new int[] {
            0, 0,
            0, 0,
            0, 0,
            0, 0,
        };

        Add(firstUserCards, secondUserCards, randomNumberOrders, 1);
    }
    public void AddGame3() {
        // Also first winner becuase random numbers are just 0
        var firstUserCards = new List<Card>() {
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Water),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Water),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Water),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Water),
        };
        var secondUserCards = new List<Card>() {
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Normal),
            new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Normal),
        };
        var randomNumberOrders = new int[] {
            0, 0,
            0, 0,
            0, 0,
            0, 0,
        };

        Add(firstUserCards, secondUserCards, randomNumberOrders, -1);
    }
    public void AddDrawnGames() {
        {
            var firstUserCards = new List<Card>() {
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Normal),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Normal),
            };
            var secondUserCards = new List<Card>() {
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Normal),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Spell, ElementType.Normal),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
            };
            var randomNumberOrders = Enumerable.Repeat(0, GameService.MAX_ROUNDS * 2).ToArray();

            Add(firstUserCards, secondUserCards, randomNumberOrders, 0);
        }

        {
            var firstUserCards = new List<Card>() {
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Fire),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Fire),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Water),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
            };
            var secondUserCards = new List<Card>() {
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Water),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal),
                new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Fire),
            };
            var randomNumberOrders = Enumerable.Repeat(0, GameService.MAX_ROUNDS * 2).ToArray();

            Add(firstUserCards, secondUserCards, randomNumberOrders, 0);
        }
    }
}

public abstract class GameServiceTests : TestsBase {
    private readonly StudRandomNumberGenerator _numberGenerator = new StudRandomNumberGenerator();
    private readonly StudCardsRepository _cardsRepository = new StudCardsRepository();

    protected override Task RegisterServicesAsync(IServiceCollection services) {
        services
            .AddSingleton<ICardsRepository>(_cardsRepository)
            .AddSingleton<IRandomNumberGenerator>(_numberGenerator);

        return Task.CompletedTask;
    }


    [Theory, Trait("Type", "Integration")]
    [ClassData(typeof(SimulatedGamesData))]
    public async Task SimulateGame(IReadOnlyList<Card> firstUserCards, IReadOnlyList<Card> secondUserCards, IReadOnlyList<int> randomNumberOrders, int winner) {
        // Arrange
        var firstUsername = "first";
        var secondUsername = "second";
        var usersService = _serviceProvider.GetRequiredService<IUsersService>();
        await usersService.RegisterAsync(firstUsername, string.Empty);
        await usersService.RegisterAsync(secondUsername, string.Empty);

        _cardsRepository._SetDeck(firstUsername, firstUserCards);
        _cardsRepository._SetDeck(secondUsername, secondUserCards);
        _numberGenerator._SetOrder(randomNumberOrders);
        var sut = _serviceProvider.GetRequiredService<IGameService>();

        // Assert
        var battle1Task = sut.EnterBattleAsync(firstUsername);
        var battle2Result = await sut.EnterBattleAsync(secondUsername);
        var battleResult = await battle1Task;

        var firstUserStats = await sut.GetStatsAsync(firstUsername);
        var secondUserStats = await sut.GetStatsAsync(secondUsername);

        // Assert
        Assert.Equal(battleResult, battle2Result); // Both games return same thing
        switch (winner) {
            case > 0:
                Assert.Equal(firstUsername, battleResult.WinnerUsername);
                Assert.Equal(new Stats(firstUsername, Stats.STARTING_ELO + Stats.ELO_WIN_GAIN, 1, 0), firstUserStats);
                Assert.Equal(new Stats(secondUsername, Stats.STARTING_ELO - Stats.ELO_LOSE_LOSS, 0, 1), secondUserStats);
                break;
            case < 0:
                Assert.Equal(secondUsername, battleResult.WinnerUsername);
                Assert.Equal(new Stats(firstUsername, Stats.STARTING_ELO - Stats.ELO_LOSE_LOSS, 0, 1), firstUserStats);
                Assert.Equal(new Stats(secondUsername, Stats.STARTING_ELO + Stats.ELO_WIN_GAIN, 1, 0), secondUserStats);
                break;
            case 0:
                Assert.True(battleResult.IsDraw);
                Assert.Equal(new Stats(firstUsername, Stats.STARTING_ELO, 0, 0), firstUserStats);
                Assert.Equal(new Stats(secondUsername, Stats.STARTING_ELO, 0, 0), secondUserStats);
                break;
        }
    }
}
