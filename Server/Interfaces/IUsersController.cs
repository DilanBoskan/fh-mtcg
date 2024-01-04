using Server.Common;

namespace Server.Interfaces;

public interface IUsersController {
    Task<HttpResponse> GetAsync(HttpRequest request);
    Task<HttpResponse> LoginAsync(HttpRequest request);
    Task<HttpResponse> RegisterAsync(HttpRequest request);
    Task<HttpResponse> UpdateAsync(HttpRequest request);
}