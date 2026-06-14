namespace AIKnowledgeAssistant.Application.Interfaces;

/// <summary>
/// Configuration for the recursive character text splitter.
/// Sizes are expressed in tokens; internally approximated as 4 characters per token.
/// </summary>
public sealed record ChunkingOptions(int ChunkSize = 512, int OverlapSize = 50)
{
    public static readonly ChunkingOptions Default = new();
}

/// <summary>A single text chunk produced by <see cref="ITextChunkingService"/>.</summary>
public sealed record TextChunk(string Text, int TokenCount, int Index);

/// <summary>
/// Splits a document's raw text into overlapping chunks suitable for embedding.
/// </summary>
public interface ITextChunkingService
{
    /// <summary>
    /// Splits <paramref name="text"/> into chunks using a recursive character splitter
    /// that tries paragraph, line, sentence, and word boundaries in order.
    /// </summary>
    IReadOnlyList<TextChunk> Split(string text, ChunkingOptions? options = null);
}
