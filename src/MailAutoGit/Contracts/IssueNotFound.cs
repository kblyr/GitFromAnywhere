namespace MailAutoGit.Contracts;

public record IssueNotFound
{
    public long RepositoryId { get; init; }
    public int Number { get; init; }
    public string Title { get; init; } = "";
}