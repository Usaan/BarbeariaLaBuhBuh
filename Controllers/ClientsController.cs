using BarbeariaLaBuhBuh.DTOs;
using BarbeariaLaBuhBuh.Models;
using BarbeariaLaBuhBuh.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarbeariaLaBuhBuh.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetAll()
    {
        var clients = await _clientService.GetAllAsync();
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetById(int id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client is null)
            return NotFound();

        return Ok((object)client);
    }

    [HttpPost]
    public async Task<ActionResult<Client>> Create(Client client)
    {
        try
        {
            var created = await _clientService.CreateAsync(client);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Client>> Update(int id, Client client)
    {
        if (id != client.Id)
            return BadRequest("ID não corresponde");

        try
        {
            var updated = await _clientService.UpdateAsync(client);
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
        var success = await _clientService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Client>> PartialUpdate(int id, UpdateClientDto dto)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client is null)
            return NotFound();

        if (dto.Name is not null) client.Name = dto.Name;
        if (dto.Email is not null) client.Email = dto.Email;
        if (dto.Phone is not null) client.Phone = dto.Phone;

        try
        {
            var updated = await _clientService.UpdateAsync(client);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
