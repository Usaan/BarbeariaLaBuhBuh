namespace BarbeariaLaBuhBuh.DTOs;

public class UpdateBarberDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? Rating { get; set; }
    public bool? IsActive { get; set; }
}
