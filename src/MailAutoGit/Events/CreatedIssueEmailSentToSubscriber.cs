namespace MailAutoGit.Events;

public record CreatedIssueEmailSentToSubscriber
{
    public long RepositoryId { get; init; }
    public int Number { get; init; }
}