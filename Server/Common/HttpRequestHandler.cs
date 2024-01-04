namespace Server.Common;

public delegate Task<HttpResponse> HttpRequestHandler(HttpRequest request);
