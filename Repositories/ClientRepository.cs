using BarbeariaLaBuhBuh.Data;
using BarbeariaLaBuhBuh.Models;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaLaBuhBuh.Repositories;

public class ClientRepository(BarbeariaDbContext context) : IClientRepository
{
    private readonly BarbeariaDbContext _context = context;

    public async Task<Client> AddAsync(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        return await _context.Clients.FindAsync(id);
    }

    public async Task<Client?> GetByGoogleIdAsync(string googleId)
    {
        return await _context.Clients.FirstOrDefaultAsync(c => c.GoogleId == googleId);
    }


    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        return await _context.Clients.ToListAsync();
    }

    public async Task<Client> UpdateAsync(Client client)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var client = await GetByIdAsync(id);
        if (client == null)
            return false;

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _context.Clients
            .AnyAsync(c => c.Email == email && (excludeId == null || c.Id != excludeId));
    }
}
