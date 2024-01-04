namespace Domain;
public record Trade(Guid Id, string OwnerUsername, Guid CardToTrade, CardType Type, int MinimumDamage);
