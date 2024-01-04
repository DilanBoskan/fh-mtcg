using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Extensions;
using Infrastructure.Repositories.InMemory;
using Infrastructure.Repositories.Sql;
using Tests.Common;

namespace Tests.Extensions;
public static class ServiceExtensions {
    public static IServiceCollection RegisterInMemoryServices(this IServiceCollection services) {
        return services
            .RegisterServerServices(string.Empty)
            .AddScoped<IUsersRepository, InMemoryUsersRepository>()
            .AddScoped<IPackagesRepository, InMemoryPackagesRepository>()
            .AddScoped<ICardsRepository, InMemoryCardsRepository>()
            .AddScoped<IMoneyRepository, InMemoryMoneyRepository>()
            .AddScoped<IStatsRepository, InMemoryStatsRepository>()
            .AddScoped<ITradingRepository, InMemoryTradingRepository>();
    }
    public static async Task<IServiceCollection> RegisterSqlServicesAsync(this IServiceCollection services, string connectionString) {
        services
            .RegisterServerServices(connectionString);

        var dbConfig = services.BuildServiceProvider().GetRequiredService<DatabaseConfiguration>();
        await dbConfig.InitializeAsync();

        return services;
    }
}
