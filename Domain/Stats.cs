namespace Domain;
public record Stats(string Username, int Elo, int Wins, int Losses) {
    public const int STARTING_ELO = 100;
    public const int ELO_WIN_GAIN = 3;
    public const int ELO_LOSE_LOSS = 5;

    public static Stats Empty(string username) => new Stats(username, STARTING_ELO, 0, 0);
}
