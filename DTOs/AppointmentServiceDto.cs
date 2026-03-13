namespace BarbeariaLaBuhBuh.DTOs
{
    public class AppointmentServiceDto
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int ServiceId { get; set; }
        public ServiceDto? Service { get; set; }
    }
}
