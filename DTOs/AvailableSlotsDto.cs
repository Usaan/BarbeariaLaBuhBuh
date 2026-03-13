namespace BarbeariaLaBuhBuh.DTOs
{
    public class AvailableSlotsDto
    {
        public string Date { get; set; } = string.Empty;
        public int BarberId { get; set; }
        public List<string> AvailableSlots { get; set; } = new();
    }
}
