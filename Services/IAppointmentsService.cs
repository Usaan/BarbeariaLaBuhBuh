using BarbeariaLaBuhBuh.DTOs;
using BarbeariaLaBuhBuh.Models;

namespace BarbeariaLaBuhBuh.Services
{
    public interface IAppointmentsService
    {
        Task<List<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int id);
        Task<List<Appointment>> GetByClientIdAsync(int clientId);
        Task<List<Appointment>> GetByBarbersIdAsync(int barberId);

        Task<AvailableSlotsDto> GetAvailableSlotsAsync(int barberId, string date, int durationMinutes);

        Task<Appointment> CreateAsync(CreateAppointmentDto dto);
        Task<Appointment> UpdateAsync(int id, UpdateAppointmentDto dto);
        Task<Appointment> UpdateStatusAsync(int id, int newStatus);
        Task DeleteAsync(int id);
    }
}
