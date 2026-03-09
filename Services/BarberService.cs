using BarbeariaLaBuhBuh.Models;
using BarbeariaLaBuhBuh.Repositories;

namespace BarbeariaLaBuhBuh.Services;

public class BarberService(IBarberRepository repository) : IBarberService
{
    private readonly IBarberRepository _repository = repository;

    public async Task<Barber> CreateAsync(Barber barber)
    {
        if (string.IsNullOrWhiteSpace(barber.FirstName))
            throw new InvalidOperationException("Nome é obrigatório");

        if (string.IsNullOrWhiteSpace(barber.LastName))
            throw new InvalidOperationException("Sobrenome é obrigatório");

        if (string.IsNullOrWhiteSpace(barber.Email))
            throw new InvalidOperationException("Email é obrigatório");

        if (string.IsNullOrWhiteSpace(barber.Phone))
            throw new InvalidOperationException("Telefone é obrigatório");

        if (!IsValidEmail(barber.Email))
            throw new InvalidOperationException("Email inválido");

        if (barber.Rating < 0 || barber.Rating > 5)
            throw new InvalidOperationException("Avaliação deve estar entre 0 e 5");

        if (await EmailExistsAsync(barber.Email))
            throw new InvalidOperationException("Email já está em uso");

        return await _repository.AddAsync(barber);
    }

    public async Task<Barber?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Barber>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<Barber>> GetActiveAsync()
    {
        return await _repository.GetActiveAsync();
    }

    public async Task<Barber> UpdateAsync(Barber barber)
    {
        if (string.IsNullOrWhiteSpace(barber.FirstName))
            throw new InvalidOperationException("Nome é obrigatório");

        if (string.IsNullOrWhiteSpace(barber.LastName))
            throw new InvalidOperationException("Sobrenome é obrigatório");

        if (string.IsNullOrWhiteSpace(barber.Email))
            throw new InvalidOperationException("Email é obrigatório");

        if (string.IsNullOrWhiteSpace(barber.Phone))
            throw new InvalidOperationException("Telefone é obrigatório");

        if (!IsValidEmail(barber.Email))
            throw new InvalidOperationException("Email inválido");

        if (barber.Rating < 0 || barber.Rating > 5)
            throw new InvalidOperationException("Avaliação deve estar entre 0 e 5");

        if (await EmailExistsAsync(barber.Email, barber.Id))
            throw new InvalidOperationException("Email já está em uso");

        return await _repository.UpdateAsync(barber);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _repository.EmailExistsAsync(email, excludeId);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
