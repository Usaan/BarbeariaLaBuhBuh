using BarbeariaLaBuhBuh.Data;
using BarbeariaLaBuhBuh.Models;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaLaBuhBuh.Repositories;

public class BarberRepository(BarbeariaDbContext context) : IBarberRepository
{
    private readonly BarbeariaDbContext _context = context;

    public async Task<Barber> AddAsync(Barber barber)
    {
        _context.Barbers.Add(barber);
        await _context.SaveChangesAsync();
        return barber;
    }

    public async Task<Barber?> GetByIdAsync(int id)
    {
        return await _context.Barbers.FindAsync(id);
    }

    public async Task<IEnumerable<Barber>> GetAllAsync()
    {
        return await _context.Barbers.ToListAsync();
    }

    public async Task<IEnumerable<Barber>> GetActiveAsync()
    {
        return await _context.Barbers
            .Where(b => b.IsActive)
            .ToListAsync();
    }

    public async Task<Barber> UpdateAsync(Barber barber)
    {
        _context.Barbers.Update(barber);
        await _context.SaveChangesAsync();
        return barber;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var barber = await GetByIdAsync(id);
        if (barber == null)
            return false;

        _context.Barbers.Remove(barber);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _context.Barbers
            .AnyAsync(b => b.Email == email && (excludeId == null || b.Id != excludeId));
    }
}
