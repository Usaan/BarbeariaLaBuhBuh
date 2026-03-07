using BarbeariaLaBuhBuh.Models;

namespace BarbeariaLaBuhBuh.Repositories;

public interface IClientRepository
{
    Task<Client> AddAsync(Client client);
    Task<Client?> GetByIdAsync(int id);
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client> UpdateAsync(Client client);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
