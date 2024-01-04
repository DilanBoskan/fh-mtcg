using Domain;
using Npgsql;

namespace Infrastructure.Repositories.Sql;

public abstract class RepositoryBase : IDisposable, IAsyncDisposable {
    protected RepositoryBase(string connectionString) {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<CardType>();
        dataSourceBuilder.MapEnum<ElementType>();

        _dataSource = dataSourceBuilder.Build();
    }


    public void Dispose() => _dataSource.Dispose();
    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();


    protected NpgsqlDataSource _dataSource;
}