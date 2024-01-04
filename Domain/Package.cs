namespace Domain;
public record Package(Guid Id, IReadOnlyList<Card> Cards);
