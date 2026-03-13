// ============================================================
// Arquivo: Services/AppointmentsService.cs (VERSÃO CORRIGIDA)
// Descrição: Serviço com lógica de negócio para agendamentos
// ============================================================

using BarbeariaLaBuhBuh.Data;
using BarbeariaLaBuhBuh.DTOs;
using BarbeariaLaBuhBuh.Models;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaLaBuhBuh.Services;

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

    /// <summary>
    /// Retorna todos os agendamentos com dados relacionados
    /// </summary>
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

    /// <summary>
    /// Retorna um agendamento específico pelo ID
    /// </summary>
    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Barber)
            .Include(a => a.AppointmentServices)
                .ThenInclude(s => s.Service)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// Retorna todos os agendamentos de um cliente
    /// </summary>
    public async Task<List<Appointment>> GetByClientIdAsync(int clientId)
    {
        // Validar se cliente existe
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

    /// <summary>
    /// Retorna todos os agendamentos de um barbeiro
    /// </summary>
    public async Task<List<Appointment>> GetByBarbersIdAsync(int barberId)
    {
        // Validar se barbeiro existe
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

    /// <summary>
    /// Retorna horários disponíveis para um barbeiro em uma data específica
    /// </summary>
    public async Task<AvailableSlotsDto> GetAvailableSlotsAsync(int barberId, string date, int durationMinutes)
    {
        // Validar data
        if (!DateTime.TryParse(date, out var parsedDate))
            throw new InvalidOperationException("Data inválida. Use o formato YYYY-MM-DD");

        // Validar se barbeiro existe e está ativo
        var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.Id == barberId);
        if (barber == null || !barber.IsActive)
            throw new InvalidOperationException($"Barbeiro com ID {barberId} não encontrado ou inativo");

        // Validar duração
        if (durationMinutes <= 0 || durationMinutes > 480) // Máximo 8 horas
            throw new InvalidOperationException("Duração inválida. Deve estar entre 1 e 480 minutos");

        // Buscar agendamentos do barbeiro na data
        // ✅ CORRIGIDO: Adicionar cast (int) ao comparar Status com AppointmentStatus
        var appointmentsOnDate = await _context.Appointments
            .Where(a => a.BarberId == barberId &&
                       a.AppointmentDateTime.Date == parsedDate.Date &&
                       (a.Status == (int)AppointmentStatus.Scheduled ||
                        a.Status == (int)AppointmentStatus.Confirmed ||
                        a.Status == (int)AppointmentStatus.InProgress))
            .Include(a => a.AppointmentServices)
            .ToListAsync();

        // Gerar lista de slots disponíveis
        var availableSlots = new List<string>();

        // Iterar por cada slot de 30 minutos
        for (int hour = OPENING_HOUR; hour < CLOSING_HOUR; hour++)
        {
            for (int minute = 0; minute < 60; minute += SLOT_DURATION_MINUTES)
            {
                var slotTime = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, hour, minute, 0);
                var slotEndTime = slotTime.AddMinutes(durationMinutes);

                // Validar se o slot termina dentro do horário comercial
                if (slotEndTime.Hour > CLOSING_HOUR || (slotEndTime.Hour == CLOSING_HOUR && slotEndTime.Minute > 0))
                    continue;

                // Verificar se há conflito com agendamentos existentes
                bool hasConflict = appointmentsOnDate.Any(a =>
                {
                    var appointmentEnd = a.AppointmentDateTime.AddMinutes(a.TotalDurationMinutes);

                    // Verificar sobreposição
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

    /// <summary>
    /// Cria um novo agendamento
    /// </summary>
    public async Task<Appointment> CreateAsync(CreateAppointmentDto dto)
    {
        // ===== VALIDAÇÕES =====

        // 1. Validar cliente
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
        if (client == null)
            throw new InvalidOperationException($"Cliente com ID {dto.ClientId} não encontrado");

        // 2. Validar barbeiro
        var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.Id == dto.BarberId && b.IsActive);
        if (barber == null)
            throw new InvalidOperationException($"Barbeiro com ID {dto.BarberId} não encontrado ou inativo");

        // 3. Validar data/hora
        if (dto.AppointmentDateTime <= DateTime.Now)
            throw new InvalidOperationException("A data e hora do agendamento devem ser no futuro");

        // 4. Validar serviços
        if (dto.AppointmentServices == null || dto.AppointmentServices.Count == 0)
            throw new InvalidOperationException("Deve haver pelo menos um serviço no agendamento");

        // 5. Buscar e validar serviços
        var serviceIds = dto.AppointmentServices.Select(s => s.ServiceId).ToList();
        var services = await _context.Services
            .Where(s => serviceIds.Contains(s.Id) && s.IsActive)
            .ToListAsync();

        if (services.Count != serviceIds.Count)
            throw new InvalidOperationException("Um ou mais serviços não foram encontrados ou estão inativos");

        // 6. Verificar disponibilidade (sem sobreposição)
        var appointmentEnd = dto.AppointmentDateTime.AddMinutes(services.Sum(s => s.DurationMinutes));

        // ✅ CORRIGIDO: Adicionar cast (int) ao comparar Status com AppointmentStatus
        var hasConflict = await _context.Appointments
            .Where(a => a.BarberId == dto.BarberId &&
                       (a.Status == (int)AppointmentStatus.Scheduled ||
                        a.Status == (int)AppointmentStatus.Confirmed ||
                        a.Status == (int)AppointmentStatus.InProgress))
            .ToListAsync().AnyAsync(a =>
            {
                var existingEnd = a.AppointmentDateTime.AddMinutes(a.TotalDurationMinutes);
                return !(appointmentEnd <= a.AppointmentDateTime || dto.AppointmentDateTime >= existingEnd);
            });

        if (hasConflict)
            throw new InvalidOperationException("Horário indisponível para o barbeiro selecionado");

        // ===== CRIAR AGENDAMENTO =====

        var appointment = new Appointment
        {
            ClientId = dto.ClientId,
            BarberId = dto.BarberId,
            AppointmentDateTime = dto.AppointmentDateTime,
            TotalDurationMinutes = services.Sum(s => s.DurationMinutes),
            TotalPrice = services.Sum(s => s.Price),
            Status = (int)AppointmentStatus.Scheduled,
            Notes = dto.Notes,
            AppointmentServices = new List<AppointmentService>()
        };

        // Adicionar serviços ao agendamento
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

    /// <summary>
    /// Atualiza um agendamento existente (apenas se status for Scheduled ou Confirmed)
    /// </summary>
    public async Task<Appointment> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        // Validar ID
        if (id != dto.Id)
            throw new InvalidOperationException("ID da rota não corresponde ao ID do corpo da requisição");

        // Buscar agendamento
        var appointment = await _context.Appointments
            .Include(a => a.AppointmentServices)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            throw new InvalidOperationException($"Agendamento com ID {id} não encontrado");

        // Validar status (só pode atualizar se estiver Scheduled ou Confirmed)
        // ✅ CORRIGIDO: Adicionar cast (int) ao comparar Status com AppointmentStatus
        if (appointment.Status != (int)AppointmentStatus.Scheduled &&
            appointment.Status != (int)AppointmentStatus.Confirmed)
            throw new InvalidOperationException($"Não é possível alterar um agendamento com status {(AppointmentStatus)appointment.Status}");

        // Validar cliente
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
        if (client == null)
            throw new InvalidOperationException($"Cliente com ID {dto.ClientId} não encontrado");

        // Validar barbeiro
        var barber = await _context.Barbers.FirstOrDefaultAsync(b => b.Id == dto.BarberId && b.IsActive);
        if (barber == null)
            throw new InvalidOperationException($"Barbeiro com ID {dto.BarberId} não encontrado ou inativo");

        // Validar data/hora
        if (dto.AppointmentDateTime <= DateTime.Now)
            throw new InvalidOperationException("A data e hora do agendamento devem ser no futuro");

        // Validar serviços
        if (dto.AppointmentServices == null || dto.AppointmentServices.Count == 0)
            throw new InvalidOperationException("Deve haver pelo menos um serviço no agendamento");

        // Buscar serviços
        var serviceIds = dto.AppointmentServices.Select(s => s.ServiceId).ToList();
        var services = await _context.Services
            .Where(s => serviceIds.Contains(s.Id) && s.IsActive)
            .ToListAsync();

        if (services.Count != serviceIds.Count)
            throw new InvalidOperationException("Um ou mais serviços não foram encontrados ou estão inativos");

        // Verificar disponibilidade (excluindo o agendamento atual)
        var appointmentEnd = dto.AppointmentDateTime.AddMinutes(services.Sum(s => s.DurationMinutes));

        // ✅ CORRIGIDO: Adicionar cast (int) ao comparar Status com AppointmentStatus
        var hasConflict = await _context.Appointments
            .Where(a => a.BarberId == dto.BarberId && a.Id != id &&
                       (a.Status == (int)AppointmentStatus.Scheduled ||
                        a.Status == (int)AppointmentStatus.Confirmed ||
                        a.Status == (int)AppointmentStatus.InProgress))
            .ToListAsync().AnyAsync(a =>
            {
                var existingEnd = a.AppointmentDateTime.AddMinutes(a.TotalDurationMinutes);
                return !(appointmentEnd <= a.AppointmentDateTime || dto.AppointmentDateTime >= existingEnd);
            });

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

    /// <summary>
    /// Atualiza apenas o status de um agendamento
    /// </summary>
    public async Task<Appointment> UpdateStatusAsync(int id, int newStatus)
    {
        // Validar status
        if (!Enum.IsDefined(typeof(AppointmentStatus), newStatus))
            throw new InvalidOperationException($"Status inválido: {newStatus}");

        // Buscar agendamento
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appointment == null)
            throw new InvalidOperationException($"Agendamento com ID {id} não encontrado");

        var currentStatus = (AppointmentStatus)appointment.Status;
        var targetStatus = (AppointmentStatus)newStatus;

        // Validar transição de status
        if (!IsValidStatusTransition(currentStatus, targetStatus))
            throw new InvalidOperationException($"Transição inválida de {currentStatus} para {targetStatus}");

        appointment.Status = newStatus;
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Status do agendamento {id} atualizado para {targetStatus}");

        return appointment;
    }

    /// <summary>
    /// Deleta um agendamento
    /// </summary>
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

    /// <summary>
    /// Valida se a transição de status é permitida
    /// </summary>
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

// ===== ENUMERAÇÃO DE STATUS =====

public enum AppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    Missed = 6
}
