using Application.Users;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Common;
public abstract class TestsBase : IAsyncLifetime {
    protected IServiceProvider _serviceProvider = null!;
    public async Task InitializeAsync() {
        var services = new ServiceCollection();
        await RegisterServicesAsync(services);

        _serviceProvider = services.BuildServiceProvider();
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected virtual Task RegisterServicesAsync(IServiceCollection services) => Task.CompletedTask;
}
