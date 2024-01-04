using Server.Attributes;
using Server.Common;
using Server.Exceptions;
using Server.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace Server.Server;


/// <summary>
/// Initializes socket and listens for http requests and sends http responses
/// </summary>
public class HttpServer {
    public const int PORT = 10001;
    public const int BACKLOG = 5;
    public const string ADMIN_USERNAME = "admin";

    private readonly Socket _socket;
    private readonly IHttpRouter _router;
    public HttpServer(IHttpRouter router) {
        _router = router;
        _socket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp
        );
        _socket.Bind(new IPEndPoint(IPAddress.Loopback, PORT));
    }

    public void Start() {
        _socket.Listen(BACKLOG);
        Task.Run(Listen);
    }

    public void Stop() {
        if (_socket.Connected) {
            _socket.Disconnect(false);
        }
        _socket.Close();
    }

    public void Listen() {
        try {
            var clientSocket = _socket.Accept();

            Task.Run(() => ReceiveAsync(clientSocket));

            Listen();
        } catch (SocketException) {
            // Triggered when socket is closed
        }
    }

    private async Task ReceiveAsync(Socket clientSocket) {
        var requestString = await ReceiveHttpRequestAsync(clientSocket);
        var request = ParseHttpRequest(requestString);

        var requestUrl = $"http://127.0.0.1:{PORT}{request.Url}";
        Console.WriteLine($"""Handling "{request.Method}" on "{requestUrl}"...""");

        HttpResponse response;
        if (_router.CanHandleHttpRequest(request)) {
            try {
                response = await _router.HandleHttpRequestAsync(request);
            } catch (Exception e) {
                response = HttpResponse.Create(HttpStatusCode.InternalServerError, e.Message);
            }
        } else {
            response = HttpResponse.Create(HttpStatusCode.NotFound);
        }

        Console.WriteLine($"""Responding "{(int)response.StatusCode} ({response.StatusCode})" to "{request.Method}" on "{requestUrl}"...""");

        clientSocket.Send(Encoding.ASCII.GetBytes(response.ToString()));
        clientSocket.Disconnect(false);
    }

    private static async Task<string> ReceiveHttpRequestAsync(Socket clientSocket) {
        // Buffer to store the response bytes
        byte[] buffer = new byte[1024];
        var stringBuilder = new StringBuilder();

        // Receive data using ReceiveAsync
        while (true) {
            ArraySegment<byte> bufferSegment = new ArraySegment<byte>(buffer);
            SocketReceiveFromResult result = await clientSocket.ReceiveFromAsync(bufferSegment, SocketFlags.None, clientSocket.RemoteEndPoint!);

            if (result.ReceivedBytes == 0)
                break;

            string receivedData = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);
            stringBuilder.Append(receivedData);

            // Assuming a newline marks the end of the HTTP request (simple approach)
            if (receivedData.Contains("\n"))
                break;
        }

        return stringBuilder.ToString();
    }
    private static HttpRequest ParseHttpRequest(string httpRequestString) {
        string[] lines = httpRequestString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        // Extract the method and query string from the first line
        string? firstLine = lines[0];
        string[] requestLineParts = firstLine.Split(' ');
        string method = requestLineParts[0].ToUpper();
        string url = requestLineParts[1];
        Dictionary<string, string> queryParams = url.Contains('?') ? ParseQueryString(GetQueryString(url)) : new Dictionary<string, string>();

        // Extract headers
        var headers = ParseHeaders(lines);
        int headerEndIndex = Array.IndexOf(lines, string.Empty);

        // Extract the body
        string body = string.Join("\n", lines.Skip(headerEndIndex + 1));


        UserAuthorization? userAuthorization = null;
        if (headers.TryGetValue("Authorization", out var authorizationValue)) {
            userAuthorization = ExtractUserAuthorizationFromAuthorizationHeaderValue(authorizationValue);
            if (userAuthorization is null)
                throw new InvalidAuthorizationValueException();
        }

        return new HttpRequest(method, url, queryParams, body, userAuthorization);
    }

    private static string GetQueryString(string url) {
        int queryStartIndex = url.IndexOf('?');
        return queryStartIndex >= 0 ? url.Substring(queryStartIndex + 1) : string.Empty;
    }

    private static Dictionary<string, string> ParseQueryString(string queryString) {
        var queryParams = new Dictionary<string, string>();
        string[] pairs = queryString.Split('&');

        foreach (var pair in pairs) {
            if (!string.IsNullOrEmpty(pair)) {
                string[] parts = pair.Split('=');
                string key = parts[0];
                string value = parts.Length > 1 ? HttpUtility.UrlDecode(parts[1]) : string.Empty;
                queryParams[key] = value;
            }
        }

        return queryParams;
    }
    private static Dictionary<string, string> ParseHeaders(string[] requestLines) {
        var headers = new Dictionary<string, string>();
        foreach (var line in requestLines) {
            if (line.Contains(':')) {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2) {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    headers[key] = value;
                }
            } else if (string.IsNullOrWhiteSpace(line)) {
                // Stop parsing headers when an empty line is encountered
                break;
            }
        }
        return headers;
    }
    private static UserAuthorization? ExtractUserAuthorizationFromAuthorizationHeaderValue(string headerValue) {
        if (string.IsNullOrEmpty(headerValue))
            return null;

        var parts = headerValue.Split(' ');
        if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            return null;

        var tokenParts = parts[1].Split("-mtcgToken");
        if (tokenParts.Length != 2)
            return null;

        var username = tokenParts[0];
        var role = username == ADMIN_USERNAME ? UserRole.Admin : UserRole.User;

        return new UserAuthorization(username, role);
    }
}
