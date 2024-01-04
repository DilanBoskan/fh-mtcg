using Application.Money;
using Application.Packages;
using Application.Users;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Tests.Money;

public abstract class MoneyServiceTests : TestsBase, IClassFixture<DockerFixture> {
    [Fact, Trait("Type", "Unit")]
    public async Task CreateUser_CheckMoneyIsStartingAmount() {
        // Arrange
        const string username = "username";
        const int expectedMoney = UsersService.STARTING_MONEY;
        var userService = _serviceProvider.GetRequiredService<IUsersService>();
        var sut = _serviceProvider.GetRequiredService<IMoneyService>();

        // Act
        await userService.RegisterAsync(username, string.Empty);
        var money = await sut.GetMoneyAsync(username);

        // Assert
        Assert.True(money.HasValue);
        Assert.Equal(expectedMoney, money.Value);
    }

    [Fact, Trait("Type", "Integration")]
    public async Task CreateUserAndBuyPackage_CheckMoney() {
        // Arrange
        const string username = "username";
        const int expectedMoney = UsersService.STARTING_MONEY - PackagesService.PACKAGE_ACQUIRE_COST;
        var userService = _serviceProvider.GetRequiredService<IUsersService>();
        var packagesService = _serviceProvider.GetRequiredService<IPackagesService>();
        await packagesService.CreateAsync([GetStudCard(), GetStudCard(), GetStudCard(), GetStudCard(), GetStudCard()]);
        var sut = _serviceProvider.GetRequiredService<IMoneyService>();

        // Act
        await userService.RegisterAsync(username, string.Empty);
        await packagesService.AcquireAsync(username);
        var money = await sut.GetMoneyAsync(username);

        // Assert
        Assert.True(money.HasValue);
        Assert.Equal(expectedMoney, money.Value);
    }

    private Card GetStudCard() => new Card(Guid.NewGuid(), string.Empty, 10, CardType.Monster, ElementType.Normal);
}
