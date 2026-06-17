using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using AIKnowledgeAssistant.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Generates model responses by calling Ollama's REST API (<c>POST /api/generate</c>).
/// </summary>
public sealed class OllamaLlmService : ITextGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaLlmService(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var request = new OllamaGenerateRequest(
            _options.ChatModel,
            prompt,
            Stream: false);

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
            cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(result?.Response))
        {
            throw new InvalidOperationException(
                $"Ollama returned an empty completion for model '{_options.ChatModel}'.");
        }

        return result.Response.Trim();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new OllamaGenerateRequest(
            _options.ChatModel,
            prompt,
            Stream: true);

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var chunk = System.Text.Json.JsonSerializer.Deserialize<OllamaGenerateChunk>(line);
            if (chunk is null)
                continue;

            if (!string.IsNullOrEmpty(chunk.Response))
                yield return chunk.Response;

            if (chunk.Done)
                yield break;
        }
    }
}

internal sealed record OllamaGenerateRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("stream")] bool Stream);

internal sealed record OllamaGenerateResponse(
    [property: JsonPropertyName("response")] string Response);

internal sealed record OllamaGenerateChunk(
    [property: JsonPropertyName("response")] string? Response,
    [property: JsonPropertyName("done")] bool Done);
