namespace MailAutoGit;

public record CurrentRepositoryOptions
{
    public string Owner { get; init; } = "";
    public string Name { get; init; } = "";
}