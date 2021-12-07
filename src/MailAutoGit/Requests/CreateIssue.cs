namespace MailAutoGit.Requests;

public record CreateIssue
{
    public long RepositoryId { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string SubscriberEmailAddress { get; init; } = "";
}
