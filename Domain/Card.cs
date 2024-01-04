using System.Text.Json.Serialization;

namespace Domain;
public record Card(Guid Id, string Name, float Damage, [property: JsonIgnore] CardType CardType, [property: JsonIgnore] ElementType ElementType);
