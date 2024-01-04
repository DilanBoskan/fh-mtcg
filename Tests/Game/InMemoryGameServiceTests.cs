using Microsoft.Extensions.DependencyInjection;
using Tests.Extensions;
using Tests.Game;

namespace Tests.Users;

[Trait("Service", "Game"), Trait("Storage", "InMemory")]
public class InMemoryGameServiceTests : GameServiceTests {
    protected override async Task RegisterServicesAsync(IServiceCollection services) {
        services
            .RegisterInMemoryServices();

        await base.RegisterServicesAsync(services);
    }
}
