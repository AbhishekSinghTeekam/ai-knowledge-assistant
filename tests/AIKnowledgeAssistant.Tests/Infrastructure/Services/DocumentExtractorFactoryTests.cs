using AIKnowledgeAssistant.Application.Interfaces;
using AIKnowledgeAssistant.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace AIKnowledgeAssistant.Tests.Infrastructure.Services;

public sealed class DocumentExtractorFactoryTests
{
    // -------------------------------------------------------------------------
    // Helpers — build factory from real or fake extractors
    // -------------------------------------------------------------------------

    private static DocumentExtractorFactory BuildFactory(params IDocumentExtractor[] extractors) =>
        new(extractors);

    private static IDocumentExtractor FakeExtractor(params string[] contentTypes)
    {
        var extractor = Substitute.For<IDocumentExtractor>();
        extractor.SupportedContentTypes.Returns(contentTypes);
        return extractor;
    }

    // -------------------------------------------------------------------------
    // GetExtractor — happy paths
    // -------------------------------------------------------------------------

    [Fact]
    public void GetExtractor_ExactMatch_ReturnsCorrectExtractor()
    {
        var pdfExtractor = FakeExtractor("application/pdf");
        var factory = BuildFactory(pdfExtractor);

        var result = factory.GetExtractor("application/pdf");

        result.Should().BeSameAs(pdfExtractor);
    }

    [Fact]
    public void GetExtractor_IsCaseInsensitive()
    {
        var extractor = FakeExtractor("application/pdf");
        var factory = BuildFactory(extractor);

        var result = factory.GetExtractor("APPLICATION/PDF");

        result.Should().BeSameAs(extractor);
    }

    [Fact]
    public void GetExtractor_ContentTypeWithParameters_StripsThemBeforeMatching()
    {
        var txtExtractor = FakeExtractor("text/plain");
        var factory = BuildFactory(txtExtractor);

        // "text/plain; charset=utf-8" — should still resolve to txtExtractor
        var result = factory.GetExtractor("text/plain; charset=utf-8");

        result.Should().BeSameAs(txtExtractor);
    }

    [Fact]
    public void GetExtractor_MultipleExtractors_ReturnsTheRightOne()
    {
        var pdfExtractor = FakeExtractor("application/pdf");
        var txtExtractor = FakeExtractor("text/plain");
        var factory = BuildFactory(pdfExtractor, txtExtractor);

        factory.GetExtractor("application/pdf").Should().BeSameAs(pdfExtractor);
        factory.GetExtractor("text/plain").Should().BeSameAs(txtExtractor);
    }

    // -------------------------------------------------------------------------
    // GetExtractor — unhappy paths
    // -------------------------------------------------------------------------

    [Fact]
    public void GetExtractor_UnknownContentType_ThrowsNotSupportedException()
    {
        var factory = BuildFactory(FakeExtractor("application/pdf"));

        var act = () => factory.GetExtractor("image/png");

        act.Should().Throw<NotSupportedException>()
            .WithMessage("*image/png*");
    }

    [Fact]
    public void GetExtractor_NoExtractorsRegistered_ThrowsNotSupportedException()
    {
        var factory = BuildFactory();

        var act = () => factory.GetExtractor("application/pdf");

        act.Should().Throw<NotSupportedException>();
    }

    // -------------------------------------------------------------------------
    // Integration — real extractor types
    // -------------------------------------------------------------------------

    [Fact]
    public void GetExtractor_RealExtractors_PdfResolves()
    {
        var factory = BuildFactory(new PdfExtractor(), new TxtExtractor(), new DocxExtractor());

        var result = factory.GetExtractor("application/pdf");

        result.Should().BeOfType<PdfExtractor>();
    }

    [Fact]
    public void GetExtractor_RealExtractors_TxtResolves()
    {
        var factory = BuildFactory(new PdfExtractor(), new TxtExtractor(), new DocxExtractor());

        var result = factory.GetExtractor("text/plain");

        result.Should().BeOfType<TxtExtractor>();
    }

    [Fact]
    public void GetExtractor_RealExtractors_DocxResolves()
    {
        var factory = BuildFactory(new PdfExtractor(), new TxtExtractor(), new DocxExtractor());

        var result = factory.GetExtractor(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        result.Should().BeOfType<DocxExtractor>();
    }
}
