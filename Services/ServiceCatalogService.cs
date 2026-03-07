using BarbeariaLaBuhBuh.Models;
using BarbeariaLaBuhBuh.Repositories;

namespace BarbeariaLaBuhBuh.Services;

public class ServiceCatalogService(IServiceRepository repository) : IServiceCatalogService
{
    private readonly IServiceRepository _repository = repository;

    public async Task<Service> CreateAsync(Service service)
    {
        if (string.IsNullOrWhiteSpace(service.Name))
            throw new InvalidOperationException("Nome é obrigatório");

        if (service.DurationMinutes <= 0)
            throw new InvalidOperationException("Duração deve ser maior que zero");

        if (service.Price < 0)
            throw new InvalidOperationException("Preço não pode ser negativo");

        return await _repository.AddAsync(service);
    }

    public async Task<Service?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<Service>> GetActiveAsync()
    {
        return await _repository.GetActiveAsync();
    }

    public async Task<Service> UpdateAsync(Service service)
    {
        if (string.IsNullOrWhiteSpace(service.Name))
            throw new InvalidOperationException("Nome é obrigatório");

        if (service.DurationMinutes <= 0)
            throw new InvalidOperationException("Duração deve ser maior que zero");

        if (service.Price < 0)
            throw new InvalidOperationException("Preço não pode ser negativo");

        return await _repository.UpdateAsync(service);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}
