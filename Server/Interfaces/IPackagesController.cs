using Server.Common;

namespace Server.Interfaces;

public interface IPackagesController {
    Task<HttpResponse> AcquireAsync(HttpRequest request);
    Task<HttpResponse> CreateAsync(HttpRequest request);
}