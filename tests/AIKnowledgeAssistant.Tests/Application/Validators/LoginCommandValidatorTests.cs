using AIKnowledgeAssistant.Application.Commands.Auth;
using AIKnowledgeAssistant.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AIKnowledgeAssistant.Tests.Application.Validators;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    // -------------------------------------------------------------------------
    // Email
    // -------------------------------------------------------------------------

    [Fact]
    public void Email_Empty_FailsValidation()
    {
        var cmd = ValidCommand() with { Email = "" };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_InvalidFormat_FailsValidation()
    {
        var cmd = ValidCommand() with { Email = "not-an-email" };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Valid_PassesValidation()
    {
        var cmd = ValidCommand();
        _validator.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // -------------------------------------------------------------------------
    // Password
    // -------------------------------------------------------------------------

    [Fact]
    public void Password_Empty_FailsValidation()
    {
        var cmd = ValidCommand() with { Password = "" };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Present_PassesValidation()
    {
        var cmd = ValidCommand();
        _validator.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.Password);
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

    private static LoginCommand ValidCommand() =>
        new("alice@example.com", "anyPassword");
}
