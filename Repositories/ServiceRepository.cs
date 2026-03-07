using BarbeariaLaBuhBuh.Data;
using BarbeariaLaBuhBuh.Models;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaLaBuhBuh.Repositories;

public class ServiceRepository(BarbeariaDbContext context) : IServiceRepository
{
    private readonly BarbeariaDbContext _context = context;

    public async Task<Service> AddAsync(Service service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        return service;
    }

    public async Task<Service?> GetByIdAsync(int id)
    {
        return await _context.Services.FindAsync(id);
    }

    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        return await _context.Services.ToListAsync();
    }

    public async Task<IEnumerable<Service>> GetActiveAsync()
    {
        return await _context.Services
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<Service> UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
        return service;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var service = await GetByIdAsync(id);
        if (service == null)
            return false;

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return true;
    }
}
