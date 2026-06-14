using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Events;
using FluentAssertions;

namespace AIKnowledgeAssistant.Tests.Domain.Entities;

public sealed class ChunkEntityTests
{
    [Fact]
    public void Create_SetsAllPropertiesCorrectly()
    {
        var documentId = Guid.NewGuid();

        var chunk = Chunk.Create(documentId, "Some text content", 0, 4);

        chunk.DocumentId.Should().Be(documentId);
        chunk.Content.Should().Be("Some text content");
        chunk.ChunkIndex.Should().Be(0);
        chunk.TokenCount.Should().Be(4);
        chunk.VectorId.Should().BeEmpty();
        chunk.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_StartsWith_NoDomainEvents()
    {
        var chunk = Chunk.Create(Guid.NewGuid(), "text", 0, 1);

        chunk.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetVectorId_PersistsVectorId_AndRaisesChunkEmbeddedEvent()
    {
        var chunk = Chunk.Create(Guid.NewGuid(), "embedded text", 2, 3);

        chunk.SetVectorId("qdrant-vec-001");

        chunk.VectorId.Should().Be("qdrant-vec-001");
        chunk.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ChunkEmbedded>();
    }

    [Fact]
    public void SetVectorId_ChunkEmbeddedEvent_ContainsCorrectIds()
    {
        var docId = Guid.NewGuid();
        var chunk = Chunk.Create(docId, "some text", 0, 2);

        chunk.SetVectorId("vec-abc");

        var evt = chunk.DomainEvents.OfType<ChunkEmbedded>().Single();
        evt.ChunkId.Should().Be(chunk.Id);
        evt.DocumentId.Should().Be(docId);
        evt.VectorId.Should().Be("vec-abc");
    }

    [Fact]
    public void Create_MultipleChunks_HaveDifferentIds()
    {
        var docId = Guid.NewGuid();
        var a = Chunk.Create(docId, "first", 0, 1);
        var b = Chunk.Create(docId, "second", 1, 1);

        a.Id.Should().NotBe(b.Id);
    }
}
