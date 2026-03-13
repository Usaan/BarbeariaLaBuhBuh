namespace BarbeariaLaBuhBuh.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int BarberId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public int TotalDurationMinutes { get; set; }
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public string? Notes { get; set; }
        public ClientDto? Client { get; set; }
        public BarbersDto? Barber { get; set; }
        public List<AppointmentServiceDto> AppointmentServices { get; set; } = new();
    }
}
