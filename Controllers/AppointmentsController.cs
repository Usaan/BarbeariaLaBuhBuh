using BarbeariaLaBuhBuh.DTOs;
using BarbeariaLaBuhBuh.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaLaBuhBuh.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentsService _appointmentsService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IAppointmentsService appointmentsService, ILogger<AppointmentsController> logger)
    {
        _appointmentsService = appointmentsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll()
    {
        try
        {
            var appointments = await _appointmentsService.GetAllAsync();
            var appointmentDtos = appointments.Select(MapToDto).ToList();
            return Ok(appointmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar agendamentos");
            return StatusCode(500, new { message = "Erro ao buscar agendamentos" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        try
        {
            var appointment = await _appointmentsService.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(new { message = $"Agendamento com ID {id} não encontrado" });

            return Ok(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar agendamento {id}");
            return StatusCode(500, new { message = "Erro ao buscar agendamento" });
        }
    }

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByClientId(int clientId)
    {
        try
        {
            var appointments = await _appointmentsService.GetByClientIdAsync(clientId);
            var appointmentDtos = appointments.Select(MapToDto).ToList();
            return Ok(appointmentDtos);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar agendamentos do cliente {clientId}");
            return StatusCode(500, new { message = "Erro ao buscar agendamentos" });
        }
    }

    [HttpGet("barber/{barberId}")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByBarberId(int barberId)
    {
        try
        {
            var appointments = await _appointmentsService.GetByBarbersIdAsync(barberId);
            var appointmentDtos = appointments.Select(MapToDto).ToList();
            return Ok(appointmentDtos);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar agendamentos do barbeiro {barberId}");
            return StatusCode(500, new { message = "Erro ao buscar agendamentos" });
        }
    }

    [HttpGet("available-slots")]
    public async Task<ActionResult<AvailableSlotsDto>> GetAvailableSlots(
        [FromQuery] int barberId,
        [FromQuery] string date,
        [FromQuery] int durationMinutes)
    {
        try
        {
            if (barberId <= 0)
                return BadRequest(new { message = "BarberId deve ser um número positivo" });

            if (string.IsNullOrWhiteSpace(date))
                return BadRequest(new { message = "Date é obrigatório (formato: YYYY-MM-DD)" });

            if (durationMinutes <= 0)
                return BadRequest(new { message = "DurationMinutes deve ser um número positivo" });

            var availableSlots = await _appointmentsService.GetAvailableSlotsAsync(barberId, date, durationMinutes);
            return Ok(availableSlots);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar horários disponíveis");
            return StatusCode(500, new { message = "Erro ao buscar horários disponíveis" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentDto dto)
    {
        try
        {
            if (dto == null)
                return BadRequest(new { message = "Corpo da requisição inválido" });

            if (dto.ClientId <= 0)
                return BadRequest(new { message = "ClientId deve ser um número positivo" });

            if (dto.BarberId <= 0)
                return BadRequest(new { message = "BarberId deve ser um número positivo" });

            if (dto.AppointmentServices == null || dto.AppointmentServices.Count == 0)
                return BadRequest(new { message = "Deve haver pelo menos um serviço" });

            var appointment = await _appointmentsService.CreateAsync(dto);
            var appointmentDto = MapToDto(appointment);

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointmentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar agendamento");
            return StatusCode(500, new { message = "Erro ao criar agendamento" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AppointmentDto>> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        try
        {
            if (dto == null)
                return BadRequest(new { message = "Corpo da requisição inválido" });

            if (id <= 0)
                return BadRequest(new { message = "ID deve ser um número positivo" });

            if (dto.ClientId <= 0)
                return BadRequest(new { message = "ClientId deve ser um número positivo" });

            if (dto.BarberId <= 0)
                return BadRequest(new { message = "BarberId deve ser um número positivo" });

            if (dto.AppointmentServices == null || dto.AppointmentServices.Count == 0)
                return BadRequest(new { message = "Deve haver pelo menos um serviço" });

            var appointment = await _appointmentsService.UpdateAsync(id, dto);
            var appointmentDto = MapToDto(appointment);

            return Ok(appointmentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao atualizar agendamento {id}");
            return StatusCode(500, new { message = "Erro ao atualizar agendamento" });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<AppointmentDto>> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        try
        {
            if (dto == null)
                return BadRequest(new { message = "Corpo da requisição inválido" });

            if (id <= 0)
                return BadRequest(new { message = "ID deve ser um número positivo" });

            if (dto.Status < 1 || dto.Status > 6)
                return BadRequest(new { message = "Status deve estar entre 1 e 6" });

            var appointment = await _appointmentsService.UpdateStatusAsync(id, dto.Status);
            var appointmentDto = MapToDto(appointment);

            return Ok(appointmentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao atualizar status do agendamento {id}");
            return StatusCode(500, new { message = "Erro ao atualizar status" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "ID deve ser um número positivo" });

            await _appointmentsService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao deletar agendamento {id}");
            return StatusCode(500, new { message = "Erro ao deletar agendamento" });
        }
    }

    // ===== MÉTODOS AUXILIARES =====
    private AppointmentDto MapToDto(Models.Appointment appointment)
    {
            return new AppointmentDto
            {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            BarberId = appointment.BarberId,
            AppointmentDateTime = appointment.AppointmentDateTime,
            TotalDurationMinutes = appointment.TotalDurationMinutes,
            TotalPrice = appointment.TotalPrice,
            Status = (int)appointment.Status,
            Notes = appointment.Notes,
            Client = appointment.Client != null ? new ClientDto
            {
                Id = appointment.Client.Id,
                FirstName = appointment.Client.FirstName,
                LastName = appointment.Client.LastName,
                Email = appointment.Client.Email,
                Phone = appointment.Client.Phone
            } : null,
            Barber = appointment.Barber != null ? new BarbersDto
            {
                Id = appointment.Barber.Id,
                FirstName = appointment.Barber.FirstName,
                LastName = appointment.Barber.LastName,
                Email = appointment.Barber.Email,
                Phone = appointment.Barber.Phone,
                Bio = appointment.Barber.Bio,
                PhotoUrl = appointment.Barber.PhotoUrl,
                Rating = appointment.Barber.Rating,
                IsActive = appointment.Barber.IsActive
            } : null,
            AppointmentServices = appointment.AppointmentServices?.Select(s => new AppointmentServiceDto
            {
                Id = 0,
                AppointmentId = s.AppointmentId,
                ServiceId = s.ServiceId,
                Service = s.Service != null ? new ServiceDto
                {
                    Id = s.Service.Id,
                    Name = s.Service.Name,
                    Description = s.Service.Description,
                    Category = (int)s.Service.Category,
                    DurationMinutes = s.Service.DurationMinutes,
                    Price = s.Service.Price,
                    PhotoUrl = s.Service.PhotoUrl,
                    IsActive = s.Service.IsActive
                } : null
            }).ToList() ?? new List<AppointmentServiceDto>()
        };
    }
}
