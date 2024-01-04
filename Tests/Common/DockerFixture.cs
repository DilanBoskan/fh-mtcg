using Npgsql;
using System.Diagnostics;

namespace Tests.Common;

public class DockerFixture : IAsyncLifetime {
    public string ConnectionString => _connectionString;

    private const string DB_NAME = "mtcgdb";
    private const string DB_USER = "user";
    private const string DB_PASSWORD = "mtcg-password";

    private const int START_PORT = 5000;
    private static int _curPort = 0;

    private readonly string _connectionString;
    private readonly string _dockerName;
    private readonly int _port;
    public DockerFixture() {
        _port = START_PORT + _curPort++;
        _dockerName = $"testpostgresql_{_port}";
        _connectionString = $"Host=localhost;Port={_port};Username={DB_USER};Password={DB_PASSWORD};Database={DB_NAME};Include Error Detail=true;";
    }

    public async Task InitializeAsync() {
        var startInfo = new ProcessStartInfo {
            FileName = "docker",
            Arguments = $"run --name {_dockerName} -e POSTGRES_DB={DB_NAME} -e POSTGRES_USER={DB_USER} -e POSTGRES_PASSWORD={DB_PASSWORD} -p {_port}:5432 -d postgres",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo)!) {
            await process.WaitForExitAsync();
        }

        // Wait until container is ready
        bool isServiceReady = false;
        int maxAttempts = 5;
        int attempt = 0;

        while (!isServiceReady && attempt < maxAttempts) {
            try {
                await using (var conn = new NpgsqlConnection(_connectionString)) {
                    await conn.OpenAsync(); // Try to open the connection
                    isServiceReady = true; // If successful, service is ready
                }
            } catch {
                // If connection fails, wait for a bit before retrying
                attempt++;
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }

        if (!isServiceReady) {
            throw new Exception("PostgreSQL service did not start in the expected time.");
        }
    }

    public async Task DisposeAsync() {
        var startInfo = new ProcessStartInfo {
            FileName = "docker",
            Arguments = $"stop {_dockerName}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo)!) {
            await process.WaitForExitAsync();
        }

        startInfo.Arguments = $"rm -f {_dockerName}";
        using (var process = Process.Start(startInfo)!) {
            await process.WaitForExitAsync();
        }
    }
}
