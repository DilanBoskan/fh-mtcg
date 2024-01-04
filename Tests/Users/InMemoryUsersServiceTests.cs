using Microsoft.Extensions.DependencyInjection;
using Tests.Extensions;

namespace Tests.Users;

[Trait("Service", "Users"), Trait("Storage", "InMemory")]
public class InMemoryUsersServiceTests : UsersServiceTests {
    protected override async Task RegisterServicesAsync(IServiceCollection services) {
        services
            .RegisterInMemoryServices();

        await base.RegisterServicesAsync(services);
    }
}
