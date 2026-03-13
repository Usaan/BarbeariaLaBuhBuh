namespace BarbeariaLaBuhBuh.DTOs
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Category { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
