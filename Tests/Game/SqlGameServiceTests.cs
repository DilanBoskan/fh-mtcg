using Infrastructure.Repositories.Sql;
using Microsoft.Extensions.DependencyInjection;
using Tests.Common;
using Tests.Extensions;
using Tests.Game;

namespace Tests.Users;

[Trait("Service", "Game"), Trait("Storage", "Sql")]
public class SqlGameServiceTests : GameServiceTests, IClassFixture<DockerFixture> {
    private readonly string _connectionString;

    public SqlGameServiceTests(DockerFixture dockerFixture) {
        _connectionString = dockerFixture.ConnectionString;
    }

    protected override async Task RegisterServicesAsync(IServiceCollection services) {
        await services
            .RegisterSqlServicesAsync(_connectionString);

        await base.RegisterServicesAsync(services);
    }

    public override async Task DisposeAsync() {
        await base.DisposeAsync();

        var dbConfig = _serviceProvider.GetRequiredService<DatabaseConfiguration>();
        await dbConfig.ClearDatabaseAsync();
    }
}
