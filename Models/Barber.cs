namespace BarbeariaLaBuhBuh.Models;

public class Barber
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
