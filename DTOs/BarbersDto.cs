namespace BarbeariaLaBuhBuh.DTOs
{
    public class BarbersDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal Rating { get; set; }
        public bool IsActive { get; set; }
    }
}
