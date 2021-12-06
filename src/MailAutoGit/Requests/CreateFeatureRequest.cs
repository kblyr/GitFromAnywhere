namespace MailAutoGit.Requests;

public record CreateFeatureRequest
{
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string NotifyEmailAddress { get; init; } = "";
}