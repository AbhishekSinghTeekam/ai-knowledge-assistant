using AIKnowledgeAssistant.Application.Commands.Conversations;
using AIKnowledgeAssistant.Application.DTOs.Conversations;
using AIKnowledgeAssistant.Application.Queries.Conversations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIKnowledgeAssistant.API.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public sealed class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new conversation for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateConversationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new CreateConversationCommand(userId.Value, request.Title ?? string.Empty);
        var response = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetMessages), new { id = response.Id }, response);
    }

    /// <summary>Lists all conversations for the authenticated user, ordered by most recently updated.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ConversationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var response = await _mediator.Send(new GetConversationsQuery(userId.Value), cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns the full message history for a conversation.</summary>
    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(ConversationMessagesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var response = await _mediator.Send(
            new GetConversationMessagesQuery(id, userId.Value),
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Sends a message to the conversation, runs the RAG pipeline and returns
    /// the full answer as JSON (non-streaming).
    /// For token-by-token SSE streaming use GET /api/chat/{id}/stream.
    /// </summary>
    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(SendMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var command = new SendMessageCommand(
            ConversationId: id,
            UserId: userId.Value,
            Question: request.Question,
            TopK: request.TopK);

        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private Guid? GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}

// ── Request body models (inlined; no dedicated DTO file needed for simple shapes) ─

public sealed record CreateConversationRequest(string? Title);

public sealed record SendMessageRequest(string Question, int TopK = 5);
