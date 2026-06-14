using AIKnowledgeAssistant.Application.Interfaces;

namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Recursive character text splitter modelled after LangChain's implementation.
///
/// Strategy (tried in order):
///   1. Double newline  — paragraph boundary
///   2. Single newline  — line boundary
///   3. ". "            — sentence boundary
///   4. " "             — word boundary
///   5. Hard character split (fallback when no separator is found)
///
/// Token estimation: 1 token ≈ 4 characters (standard GPT English heuristic).
/// </summary>
public sealed class TextChunkingService : ITextChunkingService
{
    private const int CharsPerToken = 4;

    private static readonly string[] Separators = ["\n\n", "\n", ". ", " "];

    public IReadOnlyList<TextChunk> Split(string text, ChunkingOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        options ??= ChunkingOptions.Default;

        int chunkSizeChars = options.ChunkSize * CharsPerToken;
        int overlapChars = options.OverlapSize * CharsPerToken;

        var rawChunks = new List<string>();
        SplitRecursive(text.Trim(), Separators, chunkSizeChars, overlapChars, rawChunks);

        return rawChunks
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select((c, i) => new TextChunk(c.Trim(), EstimateTokens(c), i))
            .ToList();
    }

    // -------------------------------------------------------------------------
    // Core recursive splitting
    // -------------------------------------------------------------------------

    private static void SplitRecursive(
        string text,
        string[] separators,
        int chunkSizeChars,
        int overlapChars,
        List<string> output)
    {
        if (text.Length == 0)
            return;

        // Text already fits — no further splitting needed.
        if (text.Length <= chunkSizeChars)
        {
            output.Add(text);
            return;
        }

        // Find the first separator that actually appears in the text.
        string? chosenSep = null;
        string[] remainingSeps = [];

        for (int i = 0; i < separators.Length; i++)
        {
            if (text.Contains(separators[i], StringComparison.Ordinal))
            {
                chosenSep = separators[i];
                remainingSeps = separators[(i + 1)..];
                break;
            }
        }

        // No separator found at any level — hard character split.
        if (chosenSep is null)
        {
            HardSplit(text, chunkSizeChars, overlapChars, output);
            return;
        }

        var splits = text.Split(chosenSep, StringSplitOptions.RemoveEmptyEntries);
        var goodSplits = new List<string>();

        foreach (var split in splits)
        {
            if (split.Length > chunkSizeChars)
            {
                // Flush accumulated good splits before descending into a large segment.
                MergeIntoChunks(goodSplits, chosenSep, chunkSizeChars, overlapChars, output);
                goodSplits.Clear();

                if (remainingSeps.Length > 0)
                    SplitRecursive(split, remainingSeps, chunkSizeChars, overlapChars, output);
                else
                    HardSplit(split, chunkSizeChars, overlapChars, output);
            }
            else
            {
                goodSplits.Add(split);
            }
        }

        // Flush any remaining good splits.
        MergeIntoChunks(goodSplits, chosenSep, chunkSizeChars, overlapChars, output);
    }

    // -------------------------------------------------------------------------
    // Greedy merge with overlap
    // -------------------------------------------------------------------------

    /// <summary>
    /// Greedily merges <paramref name="parts"/> (each individually ≤ chunkSizeChars)
    /// into chunks of at most <paramref name="chunkSizeChars"/>, carrying forward
    /// a trailing overlap window of at most <paramref name="overlapChars"/> characters.
    /// </summary>
    private static void MergeIntoChunks(
        IReadOnlyList<string> parts,
        string separator,
        int chunkSizeChars,
        int overlapChars,
        List<string> output)
    {
        if (parts.Count == 0)
            return;

        var window = new List<string>();
        int windowLength = 0; // total chars including separators

        foreach (var part in parts)
        {
            if (part.Length == 0)
                continue;

            // Length that this part would add to the current window.
            int addLen = window.Count == 0
                ? part.Length
                : separator.Length + part.Length;

            if (window.Count > 0 && windowLength + addLen > chunkSizeChars)
            {
                // Emit the current window as a chunk.
                output.Add(string.Join(separator, window));

                // Trim from the front, retaining trailing parts within the overlap budget.
                while (window.Count > 1)
                {
                    // Removing window[0] costs its length + one separator.
                    int removeLen = window[0].Length + separator.Length;
                    if (windowLength - removeLen > overlapChars)
                    {
                        windowLength -= removeLen;
                        window.RemoveAt(0);
                    }
                    else
                    {
                        break;
                    }
                }

                // Safety: if the single remaining part somehow exceeds the chunk size, discard it.
                // (Cannot normally happen since parts are pre-filtered, but guards against edge cases.)
                if (window.Count == 1 && window[0].Length > chunkSizeChars)
                {
                    window.Clear();
                    windowLength = 0;
                }
            }

            int priorCount = window.Count;
            window.Add(part);
            windowLength += priorCount == 0 ? part.Length : separator.Length + part.Length;
        }

        // Emit whatever remains in the window.
        if (window.Count > 0)
            output.Add(string.Join(separator, window));
    }

    // -------------------------------------------------------------------------
    // Hard character split (last resort)
    // -------------------------------------------------------------------------

    private static void HardSplit(string text, int chunkSizeChars, int overlapChars, List<string> output)
    {
        int start = 0;
        while (start < text.Length)
        {
            int end = Math.Min(start + chunkSizeChars, text.Length);
            output.Add(text[start..end]);

            if (end == text.Length)
                break;

            // Advance with overlap; guarantee forward progress even when overlapChars ≥ chunkSizeChars.
            start = Math.Max(end - overlapChars, start + 1);
        }
    }

    // -------------------------------------------------------------------------
    // Token estimation
    // -------------------------------------------------------------------------

    private static int EstimateTokens(string text) =>
        (int)Math.Ceiling(text.Length / (double)CharsPerToken);
}
