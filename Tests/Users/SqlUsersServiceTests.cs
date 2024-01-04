using Infrastructure.Repositories.Sql;
using Microsoft.Extensions.DependencyInjection;
using Tests.Common;
using Tests.Extensions;

namespace Tests.Users;

[Trait("Service", "Users"), Trait("Storage", "Sql")]
public sealed class SqlUsersServiceTests : UsersServiceTests, IClassFixture<DockerFixture> {
    private readonly string _connectionString;

    public SqlUsersServiceTests(DockerFixture dockerFixture) {
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
