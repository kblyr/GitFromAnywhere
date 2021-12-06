namespace MailAutoGit.Contracts;

public record FeatureRequestNotFound
{
    public long RepositoryId { get; init; }
    public int Number { get; init; }
    public string Title { get; init; } = "";
}