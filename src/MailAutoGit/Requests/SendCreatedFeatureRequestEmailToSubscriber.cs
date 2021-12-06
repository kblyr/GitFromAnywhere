namespace MailAutoGit.Requests;

public record SendCreatedFeatureRequestEmailToSubscriber
{
    public long RepositoryId { get; init; }
    public int Number { get; init; }
    public string Title { get; init; } = "";
    public string EmailAddress { get; init; } = "";
}