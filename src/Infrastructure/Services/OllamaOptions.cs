namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Strongly-typed configuration for the local Ollama instance.
/// Bound from the "Ollama" section of appsettings.
/// </summary>
public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>Model used for generating text embeddings (e.g. nomic-embed-text).</summary>
    public string EmbeddingModel { get; set; } = "nomic-embed-text";

    /// <summary>Model used for chat / LLM completions (e.g. llama3).</summary>
    public string ChatModel { get; set; } = "llama3";
}
