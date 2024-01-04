using Application.Cards;
using Application.Game;
using Application.Packages;
using Application.Trading;
using Application.Users;
using Domain.Repositories;
using Infrastructure.Repositories.Sql;
using Microsoft.Extensions.DependencyInjection;
using Server.Controllers;
using Server.Extensions;
using Server.Interfaces;
using Server.Server;

string dbUsername = "user";
string dbPassword = "mtcg-password";
string connectionString = $"Host=localhost; Port=5432; Username={dbUsername}; Password={dbPassword}; Database=mtcgdb; Include Error Detail=true;";
// docker run --name postgresql -e POSTGRES_DB=mtcgdb -e POSTGRES_USER=user -e POSTGRES_PASSWORD=mtcg-password -p 5432:5432 -d postgres

// Set up IoC-Container
var serviceProvider = new ServiceCollection()
            .RegisterServerServices(connectionString)
            .BuildServiceProvider();
var dbSetup = serviceProvider.GetRequiredService<DatabaseConfiguration>();
await dbSetup.InitializeAsync();

bool running = true;
Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) => {
    e.Cancel = true;
    running = false;
};

Console.WriteLine("Starting HTTP listener...");

var server = serviceProvider.GetRequiredService<HttpServer>();

server.Start();
while (running) { }
server.Stop();

Console.WriteLine("Exiting gracefully...");