namespace MailAutoGit.Events;

public record IssueCreated
{
    public long RepositoryId { get; init; }
    public int Number { get; init; }
    public string Title { get; init; } = "";
    public string SubscriberEmailAddress { get; init; } = "";
    public bool IsOpen { get; init; }
}
