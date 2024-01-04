namespace Domain.Repositories;
public interface IPackagesRepository {
    Task<IReadOnlyList<Package>> GetAsync();
    Task CreateAsync(Package package);
    Task DeleteAsync(Guid Id);
}
