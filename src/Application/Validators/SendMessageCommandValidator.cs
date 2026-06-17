using AIKnowledgeAssistant.Application.Commands.Conversations;
using FluentValidation;

namespace AIKnowledgeAssistant.Application.Validators;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("Conversation ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.")
            .MaximumLength(8000).WithMessage("Question must not exceed 8000 characters.");

        RuleFor(x => x.TopK)
            .InclusiveBetween(1, 10)
            .WithMessage("TopK must be between 1 and 10.");
    }
}
