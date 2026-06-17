using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AIKnowledgeAssistant.Application.Interfaces;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Enums;
using AIKnowledgeAssistant.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public sealed class ChatController : ControllerBase
{
    private const string SystemPrompt = """
You are an enterprise knowledge assistant.
Answer the user using only the retrieved context when it is relevant.
If the context does not contain enough information, explicitly say that you do not know.
Do not fabricate citations or facts.
Keep the answer concise, accurate, and professional.
""";

    private readonly IConversationRepository _conversationRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorRepository _vectorRepository;
    private readonly ITextGenerationService _textGenerationService;

    public ChatController(
        IConversationRepository conversationRepository,
        IEmbeddingService embeddingService,
        IVectorRepository vectorRepository,
        ITextGenerationService textGenerationService)
    {
        _conversationRepository = conversationRepository;
        _embeddingService = embeddingService;
        _vectorRepository = vectorRepository;
        _textGenerationService = textGenerationService;
    }

    [HttpGet("{id:guid}/stream")]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task Stream(
        Guid id,
        [FromQuery] string q,
        [FromQuery] int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync("Query parameter 'q' is required.", cancellationToken);
            return;
        }

        if (topK is < 1 or > 20)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync("'topK' must be between 1 and 20.", cancellationToken);
            return;
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(id, cancellationToken);
        if (conversation is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            await Response.WriteAsync("Conversation not found.", cancellationToken);
            return;
        }

        if (conversation.UserId != userId)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var question = q.Trim();

        var userMessage = Message.Create(conversation.Id, MessageRole.User, question);
        await _conversationRepository.AddMessageAsync(userMessage, cancellationToken);

        var queryEmbedding = await _embeddingService.EmbedAsync(question, cancellationToken);
        var contextChunks = await _vectorRepository.SearchAsync(queryEmbedding, topK, cancellationToken);

        var history = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .TakeLast(12)
            .Append(userMessage)
            .ToList();

        var ragPrompt = BuildRagPrompt(SystemPrompt, contextChunks, history, question);

        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.Headers.Append("X-Accel-Buffering", "no");

        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        var answerBuilder = new StringBuilder();

        try
        {
            await foreach (var token in _textGenerationService.StreamAsync(ragPrompt, cancellationToken))
            {
                if (string.IsNullOrEmpty(token))
                    continue;

                answerBuilder.Append(token);

                var payload = JsonSerializer.Serialize(new { token });
                await Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        var answer = answerBuilder.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(answer))
        {
            var assistantMessage = Message.Create(conversation.Id, MessageRole.Assistant, answer);
            await _conversationRepository.AddMessageAsync(assistantMessage, cancellationToken);

            conversation.Touch();
            await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        }
    }

    private static string BuildRagPrompt(
        string systemPrompt,
        IReadOnlyList<VectorSearchResult> contextChunks,
        IReadOnlyCollection<Message> history,
        string userQuestion)
    {
        var sb = new StringBuilder();

        sb.AppendLine("[System Prompt]");
        sb.AppendLine(systemPrompt);
        sb.AppendLine();

        sb.AppendLine("[Retrieved Context Chunks]");
        if (contextChunks.Count == 0)
        {
            sb.AppendLine("No relevant context was retrieved.");
        }
        else
        {
            for (int i = 0; i < contextChunks.Count; i++)
            {
                var chunk = contextChunks[i];
                chunk.Metadata.TryGetValue("fileName", out var fileName);

                sb.AppendLine($"Chunk {i + 1} | Score: {chunk.Score:F4} | File: {fileName ?? "unknown"}");
                sb.AppendLine(chunk.Content);
                sb.AppendLine();
            }
        }

        sb.AppendLine("[Conversation History]");
        foreach (var message in history.OrderBy(m => m.CreatedAt).TakeLast(12))
        {
            sb.AppendLine($"{message.Role}: {message.Content}");
        }
        sb.AppendLine();

        sb.AppendLine("[User Question]");
        sb.AppendLine(userQuestion);
        sb.AppendLine();

        sb.AppendLine("[Output Rules]");
        sb.AppendLine("1. Prefer retrieved context when it is relevant.");
        sb.AppendLine("2. If context is insufficient, say you do not know.");
        sb.AppendLine("3. Do not invent file names, facts, or citations.");

        return sb.ToString();
    }
}
