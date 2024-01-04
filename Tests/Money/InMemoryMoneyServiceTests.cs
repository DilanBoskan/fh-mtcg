using Microsoft.Extensions.DependencyInjection;
using Tests.Extensions;

namespace Tests.Money;

[Trait("Service", "Money"), Trait("Storage", "InMemory")]
public class InMemoryMoneyServiceTests : MoneyServiceTests {
    protected override async Task RegisterServicesAsync(IServiceCollection services) {
        services
            .RegisterInMemoryServices();

        await base.RegisterServicesAsync(services);
    }
}
