using AIKnowledgeAssistant.Application.Commands.Auth;
using AIKnowledgeAssistant.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AIKnowledgeAssistant.Tests.Application.Validators;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    // -------------------------------------------------------------------------
    // Name
    // -------------------------------------------------------------------------

    [Fact]
    public void Name_Empty_FailsValidation()
    {
        var cmd = ValidCommand() with { Name = "" };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_TooLong_FailsValidation()
    {
        var cmd = ValidCommand() with { Name = new string('a', 201) };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_Valid_PassesValidation()
    {
        var cmd = ValidCommand();
        _validator.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

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
    public void Email_TooLong_FailsValidation()
    {
        // MaximumLength(256): 251 + "@b.com"(6) = 257 chars — exceeds limit
        var cmd = ValidCommand() with { Email = new string('a', 251) + "@b.com" };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Email);
    }

    // -------------------------------------------------------------------------
    // Password
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]            // empty
    [InlineData("short1A")]     // < 8 chars
    [InlineData("alllowercase1")] // no uppercase
    [InlineData("ALLUPPERCASE1")] // no lowercase
    [InlineData("NoDigitsHere")] // no digit
    public void Password_Invalid_FailsValidation(string password)
    {
        var cmd = ValidCommand() with { Password = password };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_TooLong_FailsValidation()
    {
        var cmd = ValidCommand() with { Password = "ValidPass1" + new string('a', 120) };
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidCommand_PassesAllRules()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static RegisterCommand ValidCommand() =>
        new("Alice", "alice@example.com", "SecurePass1");
}
