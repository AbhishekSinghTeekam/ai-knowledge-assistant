using AIKnowledgeAssistant.Application.Commands.Conversations;
using FluentValidation;

namespace AIKnowledgeAssistant.Application.Validators;

public sealed class CreateConversationCommandValidator
    : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
    }
}
