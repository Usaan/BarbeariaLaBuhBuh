using BarbeariaLaBuhBuh.Models;
using BarbeariaLaBuhBuh.Repositories;

namespace BarbeariaLaBuhBuh.Services;

public class ClientService(IClientRepository repository) : IClientService
{
    private readonly IClientRepository _repository = repository;

    public async Task<Client> CreateAsync(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName))
            throw new InvalidOperationException("Nome é obrigatório");

        if (string.IsNullOrWhiteSpace(client.LastName))
            throw new InvalidOperationException("Sobrenome é obrigatório");

        if (string.IsNullOrWhiteSpace(client.Email))
            throw new InvalidOperationException("Email é obrigatório");

        if (string.IsNullOrWhiteSpace(client.Phone))
            throw new InvalidOperationException("Telefone é obrigatório");

        if (!IsValidEmail(client.Email))
            throw new InvalidOperationException("Email inválido");

        if (await EmailExistsAsync(client.Email))
            throw new InvalidOperationException("Email já está em uso");

        return await _repository.AddAsync(client);
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Client> UpdateAsync(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName))
            throw new InvalidOperationException("Nome é obrigatório");

        if (string.IsNullOrWhiteSpace(client.LastName))
            throw new InvalidOperationException("Sobrenome é obrigatório");

        if (string.IsNullOrWhiteSpace(client.Email))
            throw new InvalidOperationException("Email é obrigatório");

        if (string.IsNullOrWhiteSpace(client.Phone))
            throw new InvalidOperationException("Telefone é obrigatório");

        if (!IsValidEmail(client.Email))
            throw new InvalidOperationException("Email inválido");

        if (await EmailExistsAsync(client.Email, client.Id))
            throw new InvalidOperationException("Email já está em uso");

        return await _repository.UpdateAsync(client);
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
