using BarbeariaLaBuhBuh.Models;

namespace BarbeariaLaBuhBuh.Repositories;

public interface IBarberRepository
{
    Task<Barber> AddAsync(Barber barber);
    Task<Barber?> GetByIdAsync(int id);
    Task<IEnumerable<Barber>> GetAllAsync();
    Task<IEnumerable<Barber>> GetActiveAsync();
    Task<Barber> UpdateAsync(Barber barber);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
