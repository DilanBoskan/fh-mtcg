namespace Application.Game;
public record BattleResult(string WinnerUsername, IReadOnlyList<string> Log) {
    public bool IsDraw => string.IsNullOrEmpty(WinnerUsername);
}
