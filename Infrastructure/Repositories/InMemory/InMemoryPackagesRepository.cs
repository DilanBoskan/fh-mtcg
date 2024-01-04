using Domain;
using Domain.Repositories;

namespace Infrastructure.Repositories.InMemory;
public class InMemoryPackagesRepository : IPackagesRepository
{
    public Task<IReadOnlyList<Package>> GetAsync()
    {
        return Task.FromResult((IReadOnlyList<Package>)_packages);
    }
    public Task CreateAsync(Package package)
    {
        _packages.Add(package);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(Guid id)
    {
        _packages.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }


    private List<Package> _packages = new();
}
