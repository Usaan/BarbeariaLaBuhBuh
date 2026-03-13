using BarbeariaLaBuhBuh.Data;
using BarbeariaLaBuhBuh.DTOs;
using BarbeariaLaBuhBuh.Models;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaLaBuhBuh.Services
{
    public class AppointmentsService : IAppointmentsService
    {
        private readonly BarbeariaDbContext _context;
        private readonly ILogger<AppointmentsService> _logger;

        // Horário comercial da barbearia (configurável)
        private const int OPENING_HOUR = 9;      // 09:00
        private const int CLOSING_HOUR = 18;     // 18:00
        private const int SLOT_DURATION_MINUTES = 30; // Slots de 30 minutos

        public AppointmentsService(BarbeariaDbContext context, ILogger<AppointmentsService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Barber)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(s => s.Service)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();
        }
        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Barber)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(s => s.Service)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task<List<Appointment>> GetByClientIdAsync(int clientId)
        {
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == clientId);
            if (!clientExists)
                throw new InvalidOperationException($"Cliente com ID {clientId} não encontrado");

            return await _context.Appointments
                .Where(a => a.ClientId == clientId)
                .Include(a => a.Barber)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(s => s.Service)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();
        }
        public async Task<List<Appointment>> GetByBarbersIdAsync(int barberId)
        {
            var barberExists = await _context.Barbers.AnyAsync(b => b.Id == barberId);
            if (!barberExists)
                throw new InvalidOperationException($"Barbeiro com ID {barberId} não encontrado");

            return await _context.Appointments
                .Where(a => a.BarberId == barberId)
                .Include(a => a.Client)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(s => s.Service)
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();
        }
        public async Task<AvailableSlotsDto> GetAvailableSlotsAsync(int barberId, string date, int durationMinutes)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                throw new InvalidOperationException("Data inválida. Use o formato YYYY-MM-DD");

            var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.Id == barberId);
            if (barber == null || !barber.IsActive)
                throw new InvalidOperationException($"Barbeiro com ID {barberId} não encontrado ou inativo");

            if (durationMinutes <= 0 || durationMinutes > 480) // Máximo 8 horas
                throw new InvalidOperationException("Duração inválida. Deve estar entre 1 e 480 minutos");

            var appointmentsOnDate = await _context.Appointments
                .Where(a => a.BarberId == barberId &&
                           a.AppointmentDateTime.Date == parsedDate.Date &&
                           (a.Status == AppointmentStatus.Scheduled ||
                            a.Status == AppointmentStatus.Confirmed ||
                            a.Status == AppointmentStatus.InProgress))
                .Include(a => a.AppointmentServices)
                .ToListAsync();

            var availableSlots = new List<string>();

            for (int hour = OPENING_HOUR; hour < CLOSING_HOUR; hour++)
            {
                for (int minute = 0; minute < 60; minute += SLOT_DURATION_MINUTES)
                {
                    var slotTime = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, hour, minute, 0);
                    var slotEndTime = slotTime.AddMinutes(durationMinutes);

                    if (slotEndTime.Hour > CLOSING_HOUR || (slotEndTime.Hour == CLOSING_HOUR && slotEndTime.Minute > 0))
                        continue;

                    bool hasConflict = appointmentsOnDate.Any(a =>
                    {
                        var appointmentEnd = a.AppointmentDateTime.AddMinutes(a.TotalDurationMinutes);

                        return !(slotEndTime <= a.AppointmentDateTime || slotTime >= appointmentEnd);
                    });

                    if (!hasConflict)
                    {
                        availableSlots.Add(slotTime.ToString("HH:mm"));
                    }
                }
            }

            return new AvailableSlotsDto
            {
                Date = date,
                BarberId = barberId,
                AvailableSlots = availableSlots
            };
        }
        public async Task<Appointment> CreateAsync(CreateAppointmentDto dto)
        {
            // ===== VALIDAÇÕES =====
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
            if (client == null)
                throw new InvalidOperationException($"Cliente com ID {dto.ClientId} não encontrado");

            var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.Id == dto.BarberId && b.IsActive);
            if (barber == null)
                throw new InvalidOperationException($"Barbeiro com ID {dto.BarberId} não encontrado ou inativo");

            if (dto.AppointmentDateTime <= DateTime.Now)
                throw new InvalidOperationException("A data e hora do agendamento devem ser no futuro");

            if (dto.AppointmentServices == null || dto.AppointmentServices.Count == 0)
                throw new InvalidOperationException("Deve haver pelo menos um serviço no agendamento");

            var serviceIds = dto.AppointmentServices.Select(s => s.ServiceId).ToList();
            var services = await _context.Services
                .Where(s => serviceIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            if (services.Count != serviceIds.Count)
                throw new InvalidOperationException("Um ou mais serviços não foram encontrados ou estão inativos");

            var appointmentEnd = dto.AppointmentDateTime.AddMinutes(services.Sum(s => s.DurationMinutes));

            var hasConflict = await _context.Appointments
                .Where(a => a.BarberId == dto.BarberId &&
                           (a.Status == AppointmentStatus.Scheduled ||
                            a.Status == AppointmentStatus.Confirmed ||
                            a.Status == AppointmentStatus.InProgress))
                .AnyAsync(a => !(appointmentEnd <= a.AppointmentDateTime || dto.AppointmentDateTime >= a.AppointmentDateTime.AddMinutes(a.TotalDurationMinutes)));

            if (hasConflict)
                throw new InvalidOperationException("Horário indisponível para o barbeiro selecionado");

            var appointment = new Appointment
            {
                ClientId = dto.ClientId,
                BarberId = dto.BarberId,
                AppointmentDateTime = dto.AppointmentDateTime,
                TotalDurationMinutes = services.Sum(s => s.DurationMinutes),
                TotalPrice = services.Sum(s => s.Price),
                Status = AppointmentStatus.Scheduled,
                Notes = dto.Notes,
                AppointmentServices = new List<AppointmentService>()
            };

            foreach (var service in services)
            {
                appointment.AppointmentServices.Add(new AppointmentService
                {
                    ServiceId = service.Id,
                    Service = service
                });
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Agendamento criado: ID {appointment.Id}, Cliente {dto.ClientId}, Barbeiro {dto.BarberId}");

            return appointment;
        }
        public async Task<Appointment> UpdateAsync(int id, UpdateAppointmentDto dto)
        {
            if (id != dto.Id)
                throw new InvalidOperationException("ID da rota não corresponde ao ID do corpo da requisição");

            var appointment = await _context.Appointments
                .Include(a => a.AppointmentServices)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                throw new InvalidOperationException($"Agendamento com ID {id} não encontrado");

            if (appointment.Status != AppointmentStatus.Scheduled &&
                appointment.Status != AppointmentStatus.Confirmed)
                throw new InvalidOperationException($"Não é possível alterar um agendamento com status {(AppointmentStatus)appointment.Status}");

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
            if (client == null)
                throw new InvalidOperationException($"Cliente com ID {dto.ClientId} não encontrado");

            var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.Id == dto.BarberId && b.IsActive);
            if (barber == null)
                throw new InvalidOperationException($"Barbeiro com ID {dto.BarberId} não encontrado ou inativo");

            if (dto.AppointmentDateTime <= DateTime.Now)
                throw new InvalidOperationException("A data e hora do agendamento devem ser no futuro");

            if (dto.AppointmentServices == null || dto.AppointmentServices.Count == 0)
                throw new InvalidOperationException("Deve haver pelo menos um serviço no agendamento");

            var serviceIds = dto.AppointmentServices.Select(s => s.ServiceId).ToList();
            var services = await _context.Services
                .Where(s => serviceIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            if (services.Count != serviceIds.Count)
                throw new InvalidOperationException("Um ou mais serviços não foram encontrados ou estão inativos");

            // Verificar disponibilidade (excluindo o agendamento atual)
            var appointmentEnd = dto.AppointmentDateTime.AddMinutes(services.Sum(s => s.DurationMinutes));

            var hasConflict = await _context.Appointments
                .Where(a => a.BarberId == dto.BarberId && a.Id != id &&
                           (a.Status == AppointmentStatus.Scheduled ||
                            a.Status == AppointmentStatus.Confirmed ||
                            a.Status == AppointmentStatus.InProgress))
                .AnyAsync(a => !(appointmentEnd <= a.AppointmentDateTime || dto.AppointmentDateTime >= a.AppointmentDateTime.AddMinutes(a.TotalDurationMinutes)));

            if (hasConflict)
                throw new InvalidOperationException("Horário indisponível para o barbeiro selecionado");

            // ===== ATUALIZAR AGENDAMENTO =====

            appointment.ClientId = dto.ClientId;
            appointment.BarberId = dto.BarberId;
            appointment.AppointmentDateTime = dto.AppointmentDateTime;
            appointment.TotalDurationMinutes = services.Sum(s => s.DurationMinutes);
            appointment.TotalPrice = services.Sum(s => s.Price);
            appointment.Notes = dto.Notes;

            // Remover serviços antigos
            _context.AppointmentServices.RemoveRange(appointment.AppointmentServices);

            // Adicionar novos serviços
            appointment.AppointmentServices = services.Select(s => new AppointmentService
            {
                ServiceId = s.Id,
                Service = s
            }).ToList();

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Agendamento atualizado: ID {id}");

            return appointment;
        }
        public async Task<Appointment> UpdateStatusAsync(int id, int newStatus)
        {
            if (!Enum.IsDefined(typeof(AppointmentStatus), newStatus))
                throw new InvalidOperationException($"Status inválido: {newStatus}");

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                throw new InvalidOperationException($"Agendamento com ID {id} não encontrado");

            var currentStatus = (AppointmentStatus)appointment.Status;
            var targetStatus = (AppointmentStatus)newStatus;

            if (!IsValidStatusTransition(currentStatus, targetStatus))
                throw new InvalidOperationException($"Transição inválida de {currentStatus} para {targetStatus}");

            appointment.Status = (AppointmentStatus)newStatus;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Status do agendamento {id} atualizado para {targetStatus}");

            return appointment;
        }
        public async Task DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                throw new InvalidOperationException($"Agendamento com ID {id} não encontrado");

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Agendamento {id} deletado");
        }

        // ===== MÉTODOS AUXILIARES =====
        private bool IsValidStatusTransition(AppointmentStatus from, AppointmentStatus to)
        {
            // Estados finais não podem transicionar
            if (from == AppointmentStatus.Completed ||
                from == AppointmentStatus.Cancelled ||
                from == AppointmentStatus.Missed)
                return false;

            // Transições válidas
            return (from, to) switch
            {
                (AppointmentStatus.Scheduled, AppointmentStatus.Confirmed) => true,
                (AppointmentStatus.Scheduled, AppointmentStatus.Cancelled) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.InProgress) => true,
                (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
                (AppointmentStatus.InProgress, AppointmentStatus.Completed) => true,
                _ => false
            };
        }
    }
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
