using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Domain;

[JsonConverter(typeof(StringEnumConverter))]
public enum CardType {
    [EnumMember(Value = "monster")]
    Monster,
    [EnumMember(Value = "spell")]
    Spell
}
