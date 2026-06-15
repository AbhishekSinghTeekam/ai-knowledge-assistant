using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AIKnowledgeAssistant.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Generates text embeddings by calling the Ollama REST API (<c>POST /api/embeddings</c>)
/// with the <c>nomic-embed-text</c> model (or whatever is configured in <see cref="OllamaOptions"/>).
/// </summary>
public sealed class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaEmbeddingService> _logger;

    public OllamaEmbeddingService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new OllamaEmbeddingRequest(_options.EmbeddingModel, text);

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/embeddings", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(
            cancellationToken: cancellationToken);

        if (result?.Embedding is null || result.Embedding.Length == 0)
            throw new InvalidOperationException(
                $"Ollama returned an empty embedding for model '{_options.EmbeddingModel}'.");

        return result.Embedding;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Ollama's embeddings endpoint is single-text-per-request, so this method sends
    /// requests sequentially to avoid saturating the local model server.
    /// Results are returned in the same order as <paramref name="texts"/>.
    /// </remarks>
    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating embeddings for {Count} chunk(s) using model '{Model}'.",
            texts.Count,
            _options.EmbeddingModel);

        var embeddings = new float[texts.Count][];

        for (int i = 0; i < texts.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            embeddings[i] = await EmbedAsync(texts[i], cancellationToken);
        }

        _logger.LogInformation(
            "Successfully generated {Count} embedding(s). Vector dimensions: {Dims}.",
            embeddings.Length,
            embeddings.Length > 0 ? embeddings[0].Length : 0);

        return embeddings;
    }
}

// ---------------------------------------------------------------------------
// Internal Ollama API DTOs — not part of the public surface
// ---------------------------------------------------------------------------

internal sealed record OllamaEmbeddingRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("prompt")] string Prompt);

internal sealed record OllamaEmbeddingResponse(
    [property: JsonPropertyName("embedding")] float[] Embedding);
