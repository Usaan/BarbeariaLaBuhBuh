namespace BarbeariaLaBuhBuh.Models;

public class Appointment
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int BarberId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public int TotalDurationMinutes { get; set; }
    public decimal TotalPrice { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string Notes { get; set; } = string.Empty;

    public Client Client { get; set; } = null!;
    public Barber Barber { get; set; } = null!;
    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}

public enum AppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    Missed = 6
}
