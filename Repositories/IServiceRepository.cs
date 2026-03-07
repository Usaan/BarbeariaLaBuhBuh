using BarbeariaLaBuhBuh.Models;

namespace BarbeariaLaBuhBuh.Repositories;

public interface IServiceRepository
{
    Task<Service> AddAsync(Service service);
    Task<Service?> GetByIdAsync(int id);
    Task<IEnumerable<Service>> GetAllAsync();
    Task<IEnumerable<Service>> GetActiveAsync();
    Task<Service> UpdateAsync(Service service);
    Task<bool> DeleteAsync(int id);
}
