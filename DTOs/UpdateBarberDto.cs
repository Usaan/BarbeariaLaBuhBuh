namespace BarbeariaLaBuhBuh.DTOs;

public class UpdateBarberDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? Rating { get; set; }
    public bool? IsActive { get; set; }
}
