using BarbeariaLaBuhBuh.DTOs;
using BarbeariaLaBuhBuh.Models;
using BarbeariaLaBuhBuh.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaLaBuhBuh.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _serviceService;

    public ServicesController(IServiceCatalogService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Service>>> GetAll()
    {
        var services = await _serviceService.GetAllAsync();
        return Ok(services);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Service>>> GetActive()
    {
        var services = await _serviceService.GetActiveAsync();
        return Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Service>> GetById(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
            return NotFound();

        return Ok(service);
    }

    [HttpPost]
    public async Task<ActionResult<Service>> Create(Service service)
    {
        try
        {
            var created = await _serviceService.CreateAsync(service);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Service>> Update(int id, Service service)
    {
        if (id != service.Id)
            return BadRequest("ID não corresponde");

        try
        {
            var updated = await _serviceService.UpdateAsync(service);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _serviceService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Service>> PartialUpdate(int id, UpdateServiceDto dto)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service is null)
            return NotFound();

        if (dto.Name is not null) service.Name = dto.Name;
        if (dto.Description is not null) service.Description = dto.Description;
        if (dto.DurationMinutes is not null) service.DurationMinutes = dto.DurationMinutes.Value;
        if (dto.Price is not null) service.Price = dto.Price.Value;
        if (dto.IsActive is not null) service.IsActive = dto.IsActive.Value;

        try
        {
            var updated = await _serviceService.UpdateAsync(service);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
