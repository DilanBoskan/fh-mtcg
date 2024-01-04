using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Server.Helpers;
public static class Serializer {
    public static bool TryDeserialize<T>(string body, [NotNullWhen(true)] out T? value) {
        try {
            value = JsonConvert.DeserializeObject<T>(body);
            if (value is not null)
                return true;
        } catch { }

        value = default;
        return false;
    }

    public static string Serialize<T>(T value) {
        if (value is string valueStr)
            return valueStr;

        return JsonConvert.SerializeObject(value, Formatting.Indented);
    }
}
