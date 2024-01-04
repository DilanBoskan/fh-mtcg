using Server.Common;

namespace Server.Interfaces;

public interface ITradingController {
    Task<HttpResponse> CreateAsync(HttpRequest request);
    Task<HttpResponse> DeleteAsync(HttpRequest request);
    Task<HttpResponse> GetAsync(HttpRequest request);
    Task<HttpResponse> TradeAsync(HttpRequest request);
}