using Server.Attributes;

namespace Server.Common;

public record UserAuthorization(string Username, UserRole Role);
public record HttpRequest(string Method, string Url, Dictionary<string, string> QueryParams, string Body, UserAuthorization? Authorization);
