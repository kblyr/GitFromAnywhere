namespace MailAutoGit.Events;

public record CreatedFeatureRequestEmailSentToSubscriber
{
    public long RepositoryId { get; init; }
    public int Number { get; init; }
}