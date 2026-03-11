using Microsoft.AspNetCore.Mvc;
using BarbeariaLaBuhBuh.Services;
using BarbeariaLaBuhBuh.Models;
using Google.Apis.Auth;

namespace BarbeariaLaBuhBuh.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IClientService clientService, IConfiguration configuration) : ControllerBase
{
    private readonly IClientService _clientService = clientService;
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("google-verify")]
    public async Task<IActionResult> GoogleVerify([FromBody] GoogleTokenRequest request)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_configuration["Authentication:Google:ClientId"]!]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, settings);

            var existingClient = await _clientService.GetByGoogleIdAsync(payload.Subject);

            if (existingClient is not null)
            {
                return Ok(new
                {
                    isNewUser = false,
                    client = new
                    {
                        existingClient.Id,
                        existingClient.FirstName,
                        existingClient.LastName,
                        existingClient.Email,
                        existingClient.Phone,
                        existingClient.GooglePictureUrl
                    }
                });
            }

            return Ok(new
            {
                isNewUser = true,
                googleData = new
                {
                    googleId = payload.Subject,
                    email = payload.Email,
                    firstName = payload.GivenName,
                    lastName = payload.FamilyName,
                    pictureUrl = payload.Picture
                }
            });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { message = "Token inválido" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var existingClient = await _clientService.GetByGoogleIdAsync(request.GoogleId);
            if (existingClient is not null)
                return BadRequest(new { message = "Cliente já cadastrado" });

            var client = new Client
            {
                GoogleId = request.GoogleId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                GooglePictureUrl = request.PictureUrl
            };

            var createdClient = await _clientService.CreateAsync(client);

            return Ok(new
            {
                message = "Cliente cadastrado com sucesso",
                client = new
                {
                    createdClient.Id,
                    createdClient.FirstName,
                    createdClient.LastName,
                    createdClient.Email,
                    createdClient.Phone,
                    createdClient.GooglePictureUrl
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record GoogleTokenRequest(string Token);

public record RegisterRequest(
    string GoogleId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? PictureUrl
);
