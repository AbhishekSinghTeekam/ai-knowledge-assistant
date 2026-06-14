using AIKnowledgeAssistant.Application.Commands.Documents;
using FluentValidation;

namespace AIKnowledgeAssistant.Application.Validators;

public sealed class IngestDocumentCommandValidator : AbstractValidator<IngestDocumentCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "text/plain",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    public IngestDocumentCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(512).WithMessage("File name must not exceed 512 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage($"Allowed content types: {string.Join(", ", AllowedContentTypes)}.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File must not be empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage($"File must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");

        RuleFor(x => x.FileContent)
            .NotNull().WithMessage("File content is required.")
            .Must(c => c.Length > 0).WithMessage("File content must not be empty.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
