namespace BarbeariaLaBuhBuh.Models;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}

public enum ServiceCategory
{
    Corte = 1,
    Barba = 2,
    CorteBarba = 3,
    Coloracao = 4,
    Luzes = 5,
    Sobrancelha = 6,
    Hidratacao = 7,
    TratamentoCapilar = 8,
    Relaxamento = 9,
    Platinado = 10,
    Pigmentacao = 11,
    LimpezaPele = 12
}