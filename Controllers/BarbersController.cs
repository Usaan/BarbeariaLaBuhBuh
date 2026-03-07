using BarbeariaLaBuhBuh.DTOs;
using BarbeariaLaBuhBuh.Models;
using BarbeariaLaBuhBuh.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaLaBuhBuh.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BarbersController(IBarberService barberService) : ControllerBase
{
    private readonly IBarberService _barberService = barberService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Barber>>> GetAll()
    {
        var barbers = await _barberService.GetAllAsync();
        return Ok(barbers);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Barber>>> GetActive()
    {
        var barbers = await _barberService.GetActiveAsync();
        return Ok(barbers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Barber>> GetById(int id)
    {
        var barber = await _barberService.GetByIdAsync(id);
        if (barber == null)
            return NotFound();

        return Ok(barber);
    }

    [HttpPost]
    public async Task<ActionResult<Barber>> Create(Barber barber)
    {
        try
        {
            var created = await _barberService.CreateAsync(barber);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Barber>> Update(int id, Barber barber)
    {
        if (id != barber.Id)
            return BadRequest("ID não corresponde");

        try
        {
            var updated = await _barberService.UpdateAsync(barber);
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
        var success = await _barberService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Barber>> PartialUpdate(int id, UpdateBarberDto dto)
    {
        var barber = await _barberService.GetByIdAsync(id);
        if (barber is null)
            return NotFound();

        if (dto.Name is not null) barber.Name = dto.Name;
        if (dto.Email is not null) barber.Email = dto.Email;
        if (dto.Phone is not null) barber.Phone = dto.Phone;
        if (dto.Bio is not null) barber.Bio = dto.Bio;
        if (dto.PhotoUrl is not null) barber.PhotoUrl = dto.PhotoUrl;
        if (dto.Rating is not null) barber.Rating = dto.Rating.Value;
        if (dto.IsActive is not null) barber.IsActive = dto.IsActive.Value;

        try
        {
            var updated = await _barberService.UpdateAsync(barber);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
