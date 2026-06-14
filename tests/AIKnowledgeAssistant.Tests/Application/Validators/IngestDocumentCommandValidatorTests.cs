using System.Text;
using AIKnowledgeAssistant.Application.Commands.Documents;
using AIKnowledgeAssistant.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AIKnowledgeAssistant.Tests.Application.Validators;

public sealed class IngestDocumentCommandValidatorTests
{
    private readonly IngestDocumentCommandValidator _validator = new();

    // -------------------------------------------------------------------------
    // FileName
    // -------------------------------------------------------------------------

    [Fact]
    public void FileName_Empty_FailsValidation()
    {
        var cmd = ValidCommand() with { FileName = "" };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void FileName_TooLong_FailsValidation()
    {
        var cmd = ValidCommand() with { FileName = new string('a', 513) + ".pdf" };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.FileName);
    }

    // -------------------------------------------------------------------------
    // ContentType
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public void ContentType_Allowed_PassesValidation(string contentType)
    {
        var cmd = ValidCommand() with { ContentType = contentType };
        _validator.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.ContentType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("image/png")]
    [InlineData("application/octet-stream")]
    public void ContentType_NotAllowed_FailsValidation(string contentType)
    {
        var cmd = ValidCommand() with { ContentType = contentType };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    // -------------------------------------------------------------------------
    // FileSizeBytes
    // -------------------------------------------------------------------------

    [Fact]
    public void FileSizeBytes_Zero_FailsValidation()
    {
        var cmd = ValidCommand() with { FileSizeBytes = 0 };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.FileSizeBytes);
    }

    [Fact]
    public void FileSizeBytes_ExceedsLimit_FailsValidation()
    {
        const long overLimit = 20L * 1024 * 1024 + 1;
        var cmd = ValidCommand() with { FileSizeBytes = overLimit };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.FileSizeBytes);
    }

    [Fact]
    public void FileSizeBytes_AtLimit_PassesValidation()
    {
        const long atLimit = 20L * 1024 * 1024;
        var cmd = ValidCommand() with { FileSizeBytes = atLimit };
        _validator.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.FileSizeBytes);
    }

    // -------------------------------------------------------------------------
    // FileContent
    // -------------------------------------------------------------------------

    [Fact]
    public void FileContent_Empty_FailsValidation()
    {
        var cmd = ValidCommand() with { FileContent = [] };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.FileContent);
    }

    // -------------------------------------------------------------------------
    // UserId
    // -------------------------------------------------------------------------

    [Fact]
    public void UserId_Empty_FailsValidation()
    {
        var cmd = ValidCommand() with { UserId = Guid.Empty };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.UserId);
    }

    // -------------------------------------------------------------------------
    // Full valid command
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidCommand_PassesAllRules()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static IngestDocumentCommand ValidCommand() => new(
        FileName: "report.pdf",
        ContentType: "application/pdf",
        FileSizeBytes: 1024,
        FileContent: Encoding.UTF8.GetBytes("fake pdf content"),
        UserId: Guid.NewGuid());
}
