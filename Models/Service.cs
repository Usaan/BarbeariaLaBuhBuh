namespace BarbeariaLaBuhBuh.Models;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}
