using BarbeariaLaBuhBuh.Models;

namespace BarbeariaLaBuhBuh.Services;

public interface IServiceCatalogService
{
    Task<Service> CreateAsync(Service service);
    Task<Service?> GetByIdAsync(int id);
    Task<IEnumerable<Service>> GetAllAsync();
    Task<IEnumerable<Service>> GetActiveAsync();
    Task<Service> UpdateAsync(Service service);
    Task<bool> DeleteAsync(int id);
}
