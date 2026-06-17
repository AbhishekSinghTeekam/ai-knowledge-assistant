using System.Text;
using AIKnowledgeAssistant.Application.Commands.Conversations;
using AIKnowledgeAssistant.Application.DTOs.Conversations;
using AIKnowledgeAssistant.Application.Interfaces;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Enums;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;
using Microsoft.SemanticKernel;

namespace AIKnowledgeAssistant.Application.Handlers.Conversations;

public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResponse>
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
    private readonly Kernel _kernel;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IEmbeddingService embeddingService,
        IVectorRepository vectorRepository,
        ITextGenerationService textGenerationService,
        Kernel kernel)
    {
        _conversationRepository = conversationRepository;
        _embeddingService = embeddingService;
        _vectorRepository = vectorRepository;
        _textGenerationService = textGenerationService;
        _kernel = kernel;
    }

    public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(
            request.ConversationId,
            cancellationToken);

        if (conversation is null)
            throw new KeyNotFoundException($"Conversation '{request.ConversationId}' was not found.");

        if (conversation.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not have access to this conversation.");

        var trimmedQuestion = request.Question.Trim();

        var userMessage = Message.Create(conversation.Id, MessageRole.User, trimmedQuestion);
        await _conversationRepository.AddMessageAsync(userMessage, cancellationToken);

        var queryEmbedding = await _embeddingService.EmbedAsync(trimmedQuestion, cancellationToken);
        var contextChunks = await _vectorRepository.SearchAsync(
            queryEmbedding,
            topK: request.TopK,
            cancellationToken: cancellationToken);

        var ragPrompt = BuildRagPrompt(
            systemPrompt: SystemPrompt,
            contextChunks: contextChunks,
            conversation.Messages,
            trimmedQuestion);

        var answer = await GenerateAnswerAsync(ragPrompt, cancellationToken);

        var assistantMessage = Message.Create(conversation.Id, MessageRole.Assistant, answer);
        await _conversationRepository.AddMessageAsync(assistantMessage, cancellationToken);

        conversation.Touch();
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return new SendMessageResponse(
            ConversationId: conversation.Id,
            UserMessageId: userMessage.Id,
            AssistantMessageId: assistantMessage.Id,
            Answer: answer,
            ContextChunksUsed: contextChunks.Count,
            CreatedAt: assistantMessage.CreatedAt);
    }

    private async Task<string> GenerateAnswerAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            var skAnswer = result.ToString();
            if (!string.IsNullOrWhiteSpace(skAnswer))
                return skAnswer.Trim();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Fallback to the direct Ollama text-generation service when no SK chat connector is configured.
        }

        return await _textGenerationService.GenerateAsync(prompt, cancellationToken);
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
