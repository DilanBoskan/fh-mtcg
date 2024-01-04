using Application.Users;
using Server.Attributes;
using Server.Common;
using Server.Helpers;
using Server.Interfaces;
using System.Text.RegularExpressions;

namespace Server.Controllers;

public record UserModel(string Username, string Password);
public record GetUserModel(string Username);

public class UsersController : ControllerBase, IUsersController {
    private readonly IUsersService _userService;

    public UsersController(IUsersService userService) {
        _userService = userService;
    }

    public async Task<HttpResponse> RegisterAsync(HttpRequest request) {
        if (!Serializer.TryDeserialize<UserModel>(request.Body, out var model))
            return CreateBadRequestResponse();

        // Check if already exists
        var userExists = await _userService.ExistsAsync(model.Username);
        if (userExists)
            return CreateConflictResponse();

        await _userService.RegisterAsync(model.Username, model.Password);

        return CreateCreatedResponse();
    }

    [RequiresAuthorization]
    public async Task<HttpResponse> GetAsync(HttpRequest request) {
        // Extract data
        var match = Regex.Match(request.Url, @"users\/(\w+)");
        if (!match.Success)
            return CreateBadRequestResponse();
        var username = match.Groups[1].Value;

        // Check privileges
        if (!IsPrivileged(request, username))
            return CreateUnauthorizedResponse();

        // Check if user exists
        var userExists = await _userService.ExistsAsync(username);
        if (!userExists)
            return CreateNotFoundResponse();

        var user = await _userService.GetAsync(username);

        return CreateOKResponse(user);
    }
    [RequiresAuthorization]
    public async Task<HttpResponse> UpdateAsync(HttpRequest request) {
        // Extract data
        var match = Regex.Match(request.Url, @"users\/(\w+)");
        if (!match.Success)
            return CreateBadRequestResponse();
        var username = match.Groups[1].Value;

        if (!Serializer.TryDeserialize<UserInfo>(request.Body, out var model))
            return CreateBadRequestResponse();

        // Check privileges
        if (!IsPrivileged(request, username))
            return CreateUnauthorizedResponse();

        // Check if user exists
        var userExists = await _userService.ExistsAsync(username);
        if (!userExists)
            return CreateNotFoundResponse();

        await _userService.UpdateAsync(username, model);

        return CreateOKResponse();
    }
    public async Task<HttpResponse> LoginAsync(HttpRequest request) {
        // Extract data
        if (!Serializer.TryDeserialize<UserModel>(request.Body, out var model))
            return CreateBadRequestResponse();

        // Check if user exists
        var userExists = await _userService.ExistsAsync(model.Username);
        if (!userExists)
            return CreateUnauthorizedResponse();

        var loginResult = await _userService.LoginAsync(model.Username, model.Password);
        if (!loginResult.Success)
            return CreateUnauthorizedResponse();

        return CreateOKResponse(loginResult.AccessToken);
    }
}
