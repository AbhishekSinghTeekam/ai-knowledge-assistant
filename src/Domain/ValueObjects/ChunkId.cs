namespace AIKnowledgeAssistant.Domain.ValueObjects;

public sealed record ChunkId(Guid Value)
{
    public static ChunkId New() => new(Guid.NewGuid());

    public static ChunkId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
