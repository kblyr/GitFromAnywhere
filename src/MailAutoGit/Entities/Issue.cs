namespace MailAutoGit.Entities;

public record Issue
{
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string NotifyEmailAddress { get; init; } = "";
}
