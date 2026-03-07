using BarbeariaLaBuhBuh.Repositories;
using BarbeariaLaBuhBuh.Services;

namespace BarbeariaLaBuhBuh.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IBarberRepository, BarberRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        // Services
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IBarberService, BarberService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();

        return services;
    }
}
