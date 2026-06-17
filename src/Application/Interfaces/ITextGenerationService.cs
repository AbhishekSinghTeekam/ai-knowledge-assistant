namespace AIKnowledgeAssistant.Application.Interfaces;

/// <summary>
/// Generates text responses from a configured large language model.
/// </summary>
public interface ITextGenerationService
{
    /// <summary>
    /// Generates a completion for the provided prompt.
    /// </summary>
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a completion token-by-token for the provided prompt.
    /// </summary>
    IAsyncEnumerable<string> StreamAsync(string prompt, CancellationToken cancellationToken = default);
}
