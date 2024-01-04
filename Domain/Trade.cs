namespace Domain;
public record Trade(Guid Id, Guid CardToTrade, CardType Type, int MinimumDamage) {
    public string OwnerUsername { get; set; } = string.Empty;
}
