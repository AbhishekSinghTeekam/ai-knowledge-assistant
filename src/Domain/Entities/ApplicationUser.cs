namespace AIKnowledgeAssistant.Domain.Entities;

/// <summary>
/// Extends User as the principal identity entity for ASP.NET Core Identity integration.
/// The Infrastructure layer maps this class to IdentityUser{Guid}.
/// </summary>
public sealed class ApplicationUser : User
{
    private ApplicationUser() { }

    public new static ApplicationUser Create(string name, string email, string passwordHash)
    {
        return new ApplicationUser
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }
}
