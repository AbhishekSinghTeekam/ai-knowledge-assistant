using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Enums;
using AIKnowledgeAssistant.Domain.Events;
using FluentAssertions;

namespace AIKnowledgeAssistant.Tests.Domain.Entities;

public sealed class DocumentEntityTests
{
    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_SetsAllPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();

        var doc = Document.Create("report.pdf", "application/pdf", 2048, userId);

        doc.FileName.Should().Be("report.pdf");
        doc.ContentType.Should().Be("application/pdf");
        doc.FileSizeBytes.Should().Be(2048);
        doc.UserId.Should().Be(userId);
        doc.Status.Should().Be(DocumentStatus.Pending);
        doc.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(3));
        doc.ProcessedAt.Should().BeNull();
        doc.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_StartsWith_NoDomainEvents()
    {
        var doc = Document.Create("file.txt", "text/plain", 100, Guid.NewGuid());
        doc.DomainEvents.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Status transitions
    // -------------------------------------------------------------------------

    [Fact]
    public void MarkAsProcessing_SetsStatus_ToProcessing()
    {
        var doc = Document.Create("file.txt", "text/plain", 100, Guid.NewGuid());

        doc.MarkAsProcessing();

        doc.Status.Should().Be(DocumentStatus.Processing);
    }

    [Fact]
    public void MarkAsCompleted_SetsStatusAndProcessedAt_AndRaisesDocumentIngestedEvent()
    {
        var doc = Document.Create("report.pdf", "application/pdf", 512, Guid.NewGuid());

        doc.MarkAsCompleted();

        doc.Status.Should().Be(DocumentStatus.Completed);
        doc.ProcessedAt.Should().NotBeNull();
        doc.ProcessedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(3));
        doc.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DocumentIngested>();
    }

    [Fact]
    public void MarkAsFailed_SetsStatus_ToFailed()
    {
        var doc = Document.Create("bad.pdf", "application/pdf", 100, Guid.NewGuid());

        doc.MarkAsFailed();

        doc.Status.Should().Be(DocumentStatus.Failed);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var doc = Document.Create("file.pdf", "application/pdf", 100, Guid.NewGuid());
        doc.MarkAsCompleted();
        doc.DomainEvents.Should().NotBeEmpty();

        doc.ClearDomainEvents();

        doc.DomainEvents.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Each created document has a unique Id
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_TwoDifferentDocuments_HaveDifferentIds()
    {
        var a = Document.Create("a.pdf", "application/pdf", 100, Guid.NewGuid());
        var b = Document.Create("b.pdf", "application/pdf", 100, Guid.NewGuid());

        a.Id.Should().NotBe(b.Id);
    }
}
