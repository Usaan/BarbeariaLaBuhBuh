namespace BarbeariaLaBuhBuh.Models;

public class Client
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string? GoogleId { get; set; }
    public string? GooglePictureUrl { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
