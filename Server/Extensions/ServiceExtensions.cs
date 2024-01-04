using Application.Cards;
using Application.Game;
using Application.Money;
using Application.Packages;
using Application.Trading;
using Application.Users;
using Domain.Repositories;
using Infrastructure.Repositories.InMemory;
using Infrastructure.Repositories.Sql;
using Microsoft.Extensions.DependencyInjection;
using Server.Controllers;
using Server.Interfaces;
using Server.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Extensions;
public static class ServiceExtensions {
    public static IServiceCollection RegisterServerServices(this IServiceCollection services, string connectionString) {
        services
            .AddScoped<HttpServer>()
            .AddScoped<IHttpRouter, HttpRouter>()
            .AddSingleton<ConnectionInfo>(new ConnectionInfo(connectionString))
            // Controllers
            .AddScoped<IUsersController, UsersController>()
            .AddScoped<IPackagesController, PackagesController>()
            .AddScoped<ICardsController, CardsController>()
            .AddScoped<IGameController, GameController>()
            .AddScoped<ITradingController, TradingController>()
            // Services
            .AddScoped<IUsersService, UsersService>()
            .AddScoped<IPackagesService, PackagesService>()
            .AddScoped<ICardsService, CardsService>()
            .AddScoped<IMoneyService, MoneyService>()
            .AddScoped<IGameService, GameService>()
            .AddScoped<ITradingService, TradingService>()
            .AddScoped<IRandomNumberGenerator, RandomNumberGenerator>()
            // Repositories
            .AddScoped<DatabaseConfiguration>()
            .AddScoped<IUsersRepository, UsersRepository>()
            .AddScoped<IPackagesRepository, PackagesRepository>()
            .AddScoped<ICardsRepository, CardsRepository>()
            .AddScoped<IMoneyRepository, MoneyRepository>()
            .AddScoped<IStatsRepository, StatsRepository>()
            .AddScoped<ITradingRepository, TradingRepository>();

        return services;
    }
}
