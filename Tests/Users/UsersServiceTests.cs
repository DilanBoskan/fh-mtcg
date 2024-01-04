using Application.Users;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Server.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Tests.Users;

public abstract class UsersServiceTests : TestsBase {
    [Fact, Trait("Type", "Unit")]
    public async Task RegisterAsync_WithValidCredentials() {
        // Arrange
        var username = "testuser";
        var password = "password";
        var sut = _serviceProvider.GetRequiredService<IUsersService>();

        // Act
        await sut.RegisterAsync(username, password);

        // Assert
        var userExists = await sut.ExistsAsync(username);
        Assert.True(userExists);
    }

    [Fact, Trait("Type", "Unit")]
    public async Task GetAsync_WithExistingUser() {
        // Arrange
        var username = "testuser";
        var password = "password";
        var sut = _serviceProvider.GetRequiredService<IUsersService>();

        // Act
        await sut.RegisterAsync(username, password);
        var userInfo = await sut.GetAsync(username);

        // Assert
        Assert.NotNull(userInfo);
    }
    
    [Fact, Trait("Type", "Unit")]
    public async Task ExistsAsync_ForNonExistingUser() {
        // Arrange
        var username = "testuser";
        var sut = _serviceProvider.GetRequiredService<IUsersService>();

        // Act
        var exists = await sut.ExistsAsync(username);

        // Assert
        Assert.False(exists);
    }

    [Fact, Trait("Type", "Unit")]
    public async Task UpdateAsync_WithValidData() {
        // Arrange
        var username = "testuser";
        var password = "password";
        var newName = "Mr. TestUser";
        var newBio = "newBio";
        var newImage = "image";
        var updatedUserInfo = new UserInfo(newName, newBio, newImage);
        var sut = _serviceProvider.GetRequiredService<IUsersService>();

        // Act
        await sut.RegisterAsync(username, password);
        await sut.UpdateAsync(username, updatedUserInfo);

        // Assert
        var resultUserInfo = await sut.GetAsync(username);
        Assert.Equal(newName, resultUserInfo.Name);
        Assert.Equal(newBio, resultUserInfo.Bio);
        Assert.Equal(newImage, resultUserInfo.Image);
    }

    [Fact, Trait("Type", "Unit")]
    public async Task LoginAsync_WithValidCredentials() {
        // Arrange
        var username = "testuser";
        var password = "password";
        var sut = _serviceProvider.GetRequiredService<IUsersService>();

        // Act
        await sut.RegisterAsync(username, password);
        var result = await sut.LoginAsync(username, password);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AccessToken);
    }

    [Fact, Trait("Type", "Unit")]
    public async Task LoginAsync_WithInvalidCredentials() {
        // Arrange
        var username = "testuser";
        var password = "password";
        var invalidPassword = "invalidpassword";
        var sut = _serviceProvider.GetRequiredService<IUsersService>();

        // Act
        await sut.RegisterAsync(username, password);
        var result = await sut.LoginAsync(username, invalidPassword);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.AccessToken);
    }

    [Fact, Trait("Type", "Integration")]
    public async Task RegisterAndLogin_WithValidCredentials() {
        // Arrange
        var username = "testuser";
        var password = "password";
        var sut = _serviceProvider.GetRequiredService<IUsersService>();

        // Act
        await sut.RegisterAsync(username, password);
        var loginResult = await sut.LoginAsync(username, password);

        // Assert
        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.AccessToken);
    }
}
