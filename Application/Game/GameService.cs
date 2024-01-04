using Domain;
using Domain.Repositories;

namespace Application.Game;

public class GameService : IGameService {
    private record WaitingUser(string Username, TaskCompletionSource<BattleResult> BattleTcs);
    private delegate void GameModifierDelegate(Card firstCard, Card secondCard, ref float firstCardDamage, ref float secondCardDamage);

    public const int LOBBY_WAIT_TIMEOUT_S = 10;
    public const int MAX_ROUNDS = 100;
    private readonly IStatsRepository _statsRepository;
    private readonly ICardsRepository _cardsRepository;
    private readonly IRandomNumberGenerator _numberGenerator;
    public GameService(IStatsRepository statsRepository, ICardsRepository cardsService, IRandomNumberGenerator numberGenerator) {
        _statsRepository = statsRepository;
        _cardsRepository = cardsService;
        _numberGenerator = numberGenerator;
    }

    public async Task<Stats> GetStatsAsync(string username) {
        var stats = await _statsRepository.GetAsync(username);
        ArgumentNullException.ThrowIfNull(stats);

        return stats;
    }
    public async Task<IReadOnlyList<Stats>> GetScoreboardAsync() => (await _statsRepository.GetAsync()).OrderBy(s => s.Elo).ToList();

    public async Task<BattleResult> EnterBattleAsync(string username) {
        Func<Task<BattleResult>> battleResultTask;

        lock (_lobbyMut) {
            bool userWaiting = _lobby.TryDequeue(out var lobbyUser);

            if (userWaiting) {
                // Start battle and set result of lobby user to battle result
                battleResultTask = async () => {
                    var result = await BattleAsync(lobbyUser!.Username, username);
                    lobbyUser.BattleTcs.SetResult(result);
                    return result;
                };
            } else {
                var waitingUser = new WaitingUser(username, new TaskCompletionSource<BattleResult>());

                _lobby.Enqueue(waitingUser);

                // Wait for lobby entering user to start the battle and set the result
                battleResultTask = () => waitingUser.BattleTcs.Task;
            }
        }

        var battleResult = await battleResultTask();

        return battleResult;
    }

    private async Task<BattleResult> BattleAsync(string firstUsername, string secondUsername) {
        var firstDeck = (await _cardsRepository.GetDeckAsync(firstUsername)).ToList();
        var secondDeck = (await _cardsRepository.GetDeckAsync(secondUsername)).ToList();

        if (!firstDeck.Any()) throw new ArgumentException($"{firstUsername} does not have a deck!");
        if (!secondDeck.Any()) throw new ArgumentException($"{secondUsername} does not have a deck!");

        BattleResult result;
        List<string> logs = new() {
            $"Battle starting ({firstUsername} vs {secondUsername})"
        };
        for (int roundNum = 1; ; roundNum++) {
            var firstIndex = _numberGenerator.Next(firstDeck.Count);
            var secondIndex = _numberGenerator.Next(secondDeck.Count);
            var firstChosenCard = firstDeck[firstIndex];
            var secondChosenCard = secondDeck[secondIndex];

            logs.Add($"[{roundNum}] {firstUsername}: {firstChosenCard.Name} ({firstChosenCard.Damage} Damage) vs {secondUsername}: {secondChosenCard.Name} ({secondChosenCard.Damage} Damage)...");

            // Battle
            string? roundWinner = BattleRound(firstChosenCard, secondChosenCard) switch {
                > 0 => firstUsername,
                < 0 => secondUsername,
                _ => null // Draw
            };

            logs.Add($"[{roundNum}] " + (string.IsNullOrEmpty(roundWinner) ? "Draw!" : $"Winner: {roundWinner}"));

            // Adjust cards based on winner
            if (roundWinner == firstUsername) {
                secondDeck.Remove(secondChosenCard);
                firstDeck.Add(secondChosenCard);
            } else if (roundWinner == secondUsername) {
                firstDeck.Remove(firstChosenCard);
                secondDeck.Add(firstChosenCard);
            } else {
                // Draw
            }

            // Check exit condition
            if (!firstDeck.Any()) {
                logs.Add($"[{roundNum}] Game finished... {secondUsername} won!");
                result = new BattleResult(secondUsername, logs);
                break;
            }
            if (!secondDeck.Any()) {
                logs.Add($"[{roundNum}] Game finished... {firstUsername} won!");
                result = new BattleResult(firstUsername, logs);
                break;
            }
            if (roundNum == MAX_ROUNDS) {
                // Draw
                logs.Add($"[{roundNum}] Game finished... It is a draw!");
                result = new BattleResult(string.Empty, logs);
                break;
            }
        }

        if (result.IsDraw)
            return result;

        // Update stats
        var firstUserStats = (await _statsRepository.GetAsync(firstUsername))!;
        var secondUserStats = (await _statsRepository.GetAsync(secondUsername))!;

        if (result.WinnerUsername == firstUsername) {
            firstUserStats = firstUserStats with {
                Elo = firstUserStats.Elo + Stats.ELO_WIN_GAIN,
                Wins = firstUserStats.Wins + 1
            };
            secondUserStats = secondUserStats with {
                Elo = secondUserStats.Elo - Stats.ELO_LOSE_LOSS,
                Losses = secondUserStats.Losses + 1
            };
        } else {
            firstUserStats = firstUserStats with {
                Elo = firstUserStats.Elo - Stats.ELO_LOSE_LOSS,
                Losses = firstUserStats.Losses + 1
            };
            secondUserStats = secondUserStats with {
                Elo = secondUserStats.Elo + Stats.ELO_WIN_GAIN,
                Wins = secondUserStats.Wins + 1
            };
        }

        await _statsRepository.UpdateAsync(firstUserStats);
        await _statsRepository.UpdateAsync(secondUserStats);

        return result;
    }
    private int BattleRound(Card firstCard, Card secondCard) {
        float firstCardDamage = firstCard.Damage;
        float secondCardDamage = secondCard.Damage;

        // Apply rules
        foreach (var gameRule in _gameRules) {
            gameRule(firstCard, secondCard, ref firstCardDamage, ref secondCardDamage);
        }

        return firstCardDamage.CompareTo(secondCardDamage);
    }

    private static object _lobbyMut = new();
    private static Queue<WaitingUser> _lobby = new();


    private List<GameModifierDelegate> _gameRules = new() {
        ElementTypeRuleHandle,
        SpecialitesHandle,
    };

    // Rules
    private static void ElementTypeRuleHandle(Card firstCard, Card secondCard, ref float firstCardDamage, ref float secondCardDamage) {
        if (firstCard.CardType == CardType.Monster &&
            secondCard.CardType == CardType.Monster)
            return;

        float modifier;
        switch (firstCard.ElementType) {
            case ElementType.Water:
                modifier = secondCard.ElementType switch {
                    ElementType.Fire => 2,
                    ElementType.Normal => 0.5f,
                    _ => 1
                };
                break;
            case ElementType.Fire:
                modifier = secondCard.ElementType switch {
                    ElementType.Water => 0.5f,
                    ElementType.Normal => 2,
                    _ => 1
                };
                break;
            case ElementType.Normal:
                modifier = secondCard.ElementType switch {
                    ElementType.Water => 2,
                    ElementType.Fire => 0.5f,
                    _ => 1
                };
                break;
            default:
                throw new Exception("Invalid ElementType!");
        }

        firstCardDamage *= modifier;
        secondCardDamage *= (1 / modifier);
    }
    private static void SpecialitesHandle(Card firstCard, Card secondCard, ref float firstCardDamage, ref float secondCardDamage) {
        var firstCardName = firstCard.Name.ToLowerInvariant();
        var secondCardName = secondCard.Name.ToLowerInvariant();

        // Goblins are too afraid of Dragons to attack
        if (firstCardName.Contains("goblin") &&
            secondCardName.Contains("dragon")) {
            firstCardDamage = 0;
            return;
        }
        if (firstCardName.Contains("dragon") &&
            secondCardName.Contains("goblin")) {
            secondCardDamage = 0;
            return;
        }

        // Wizzard can control Orks so they are not able to damage them
        if (firstCardName.Contains("ork") &&
            secondCardName.Contains("wizard")) {
            firstCardDamage = 0;
            return;
        }
        if (firstCardName.Contains("wizard") &&
            secondCardName.Contains("ork")) {
            secondCardDamage = 0;
            return;
        }

        // The armor of Knights is so heavy that WaterSpells make them drown them instantly
        if (firstCardName.Contains("knight") &&
            (secondCardName.Contains("water") && secondCard.CardType == CardType.Spell)) {
            firstCardDamage = 0;
            return;
        }
        if ((firstCardName.Contains("water") && firstCard.CardType == CardType.Spell) &&
            secondCardName.Contains("knight")) {
            secondCardDamage = 0;
            return;
        }

        // The Kraken is immune against spells
        if (firstCard.CardType == CardType.Spell &&
            secondCardName.Contains("kraken")) {
            firstCardDamage = 0;
            return;
        }
        if (firstCardName.Contains("kraken") &&
            secondCard.CardType == CardType.Spell) {
            secondCardDamage = 0;
            return;
        }

        // The FireElves know Dragons since they were little and can evade their attacks.
        if (firstCardName.Contains("dragon") &&
            secondCardName.Contains("fireelve")) {
            firstCardDamage = 0;
            return;
        }
        if (firstCardName.Contains("fireelve") &&
            secondCardName.Contains("dragon")) {
            secondCardDamage = 0;
            return;
        }
    }
}
