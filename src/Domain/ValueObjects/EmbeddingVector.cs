namespace AIKnowledgeAssistant.Domain.ValueObjects;

public sealed class EmbeddingVector
{
    public float[] Values { get; }

    public int Dimensions => Values.Length;

    public EmbeddingVector(float[] values)
    {
        if (values.Length == 0)
            throw new ArgumentException("Embedding vector cannot be empty.", nameof(values));

        Values = values;
    }

    public static EmbeddingVector From(float[] values) => new(values);

    public override bool Equals(object? obj) =>
        obj is EmbeddingVector other && Values.SequenceEqual(other.Values);

    public override int GetHashCode() =>
        Values.Aggregate(0, (hash, val) => HashCode.Combine(hash, val));
}
