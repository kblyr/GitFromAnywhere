namespace MailAutoGit.Events;

public record FeatureRequestCreated
{
    public int Number { get; init; }
    public string Title { get; init; } = "";
}