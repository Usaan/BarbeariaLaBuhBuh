namespace BarbeariaLaBuhBuh.DTOs;

public class UpdateServiceDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? Price { get; set; }
    public bool? IsActive { get; set; }
}
