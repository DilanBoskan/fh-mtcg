using Infrastructure.Repositories.Sql;
using Microsoft.Extensions.DependencyInjection;
using System;
using Tests.Common;
using Tests.Extensions;

namespace Tests.Money;

[Trait("Service", "Money"), Trait("Storage", "Sql")]
public class SqlMoneyServiceTests : MoneyServiceTests, IClassFixture<DockerFixture> {
    private readonly string _connectionString;

    public SqlMoneyServiceTests(DockerFixture dockerFixture) {
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
