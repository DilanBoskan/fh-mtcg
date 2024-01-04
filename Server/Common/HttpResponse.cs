using Server.Helpers;
using System.Net;
using System.Text;

namespace Server.Common;

public record HttpResponse(HttpStatusCode StatusCode, string Response, string ContentType = "application/json") {
    public static HttpResponse Create(HttpStatusCode statusCode, string response = "", string contentType = "application/json") => new HttpResponse(statusCode, response, contentType);
    public static HttpResponse Create<T>(HttpStatusCode statusCode, T response, string contentType = "application/json") => new HttpResponse(statusCode, Serializer.Serialize(response), contentType);

    public override string ToString() {
        var stringBuilder = new StringBuilder();

        // Format the status line
        stringBuilder.AppendFormat($"HTTP/1.1 {(int)StatusCode} {StatusCode}\r\n");

        // Format headers
        stringBuilder.Append($"Content-Type: {ContentType}\r\n");
        stringBuilder.Append($"Content-Length: {Response.Length}").Append("\r\n");
        stringBuilder.Append("Connection: close\r\n");

        // End of headers
        stringBuilder.Append("\r\n");

        // Append body
        stringBuilder.Append(Response);

        return stringBuilder.ToString();
    }
}
