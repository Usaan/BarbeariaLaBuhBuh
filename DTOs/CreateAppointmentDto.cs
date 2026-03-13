namespace BarbeariaLaBuhBuh.DTOs
{
    public class CreateAppointmentDto
    {
        public int ClientId { get; set; }
        public int BarberId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<CreateAppointmentServiceDto> AppointmentServices { get; set; } = new();
    }
}
