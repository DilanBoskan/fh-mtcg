using Server.Attributes;
using Server.Common;
using System.Net;

namespace Server.Controllers;
public class ControllerBase {
    protected static bool IsPrivileged(HttpRequest request, string username) {
        // Check privileges
        if (request.Authorization is null ||
            (request.Authorization.Username != username &&
            request.Authorization.Role != UserRole.Admin))
            return false;

        return true;
    }

    // 200
    protected static HttpResponse CreateOKResponse() => CreateResponse(HttpStatusCode.OK);
    protected static HttpResponse CreateOKResponse<T>(T response, string contentType = "application/json") => CreateResponse(response, HttpStatusCode.OK, contentType);
    protected static HttpResponse CreateCreatedResponse() => CreateResponse(HttpStatusCode.Created);
    protected static HttpResponse CreateNoContentResponse() => CreateResponse(HttpStatusCode.NoContent);

    // 400
    protected static HttpResponse CreateBadRequestResponse() => CreateResponse(HttpStatusCode.BadRequest);
    protected static HttpResponse CreateUnauthorizedResponse() => CreateResponse(HttpStatusCode.Unauthorized);
    protected static HttpResponse CreateForbiddenResponse() => CreateResponse(HttpStatusCode.Forbidden);
    protected static HttpResponse CreateNotFoundResponse() => CreateResponse(HttpStatusCode.NotFound);
    protected static HttpResponse CreateConflictResponse() => CreateResponse(HttpStatusCode.Conflict);

    private static HttpResponse CreateResponse(HttpStatusCode statusCode = HttpStatusCode.OK) {
        return HttpResponse.Create(statusCode);
    }
    private static HttpResponse CreateResponse<T>(T response, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "application/json") {
        return HttpResponse.Create<T>(statusCode, response, contentType);
    }
}
