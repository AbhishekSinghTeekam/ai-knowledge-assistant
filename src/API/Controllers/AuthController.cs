using AIKnowledgeAssistant.Application.Commands.Auth;
using AIKnowledgeAssistant.Application.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new RegisterCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(response);
    }
}
