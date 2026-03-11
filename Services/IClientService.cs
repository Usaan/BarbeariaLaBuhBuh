using BarbeariaLaBuhBuh.Models;

namespace BarbeariaLaBuhBuh.Services;

public interface IClientService
{
    Task<Client> CreateAsync(Client client);
    Task<Client?> GetByIdAsync(int id);
    Task<Client?> GetByGoogleIdAsync(string googleId);
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client> UpdateAsync(Client client);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
