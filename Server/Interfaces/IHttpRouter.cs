using Server.Common;

namespace Server.Interfaces;

public interface IHttpRouter {
    bool CanHandleHttpRequest(HttpRequest request);
    Task<HttpResponse> HandleHttpRequestAsync(HttpRequest request);
}