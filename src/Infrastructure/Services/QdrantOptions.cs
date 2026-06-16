namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Strongly-typed configuration for the Qdrant vector database.
/// Bound from the "Qdrant" section of appsettings.
/// </summary>
public sealed class QdrantOptions
{
    public const string SectionName = "Qdrant";

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 6334;

    public bool Https { get; set; } = false;

    public string? ApiKey { get; set; }

    /// <summary>Name of the Qdrant collection that stores document chunk vectors.</summary>
    public string CollectionName { get; set; } = "knowledge_chunks";
}
