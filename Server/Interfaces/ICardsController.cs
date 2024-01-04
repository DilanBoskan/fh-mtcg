using Server.Common;

namespace Server.Interfaces;

public interface ICardsController {
    Task<HttpResponse> GetAsync(HttpRequest request);
    Task<HttpResponse> GetDeckAsync(HttpRequest request);
    Task<HttpResponse> SetDeckAsync(HttpRequest request);
}