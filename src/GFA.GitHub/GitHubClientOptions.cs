namespace GFA;

public record GitHubClientOptions
{
    public string ProductHeader { get; init; } = "GFA";
    public GitHubClientAuthenticationMode AuthenticationMode { get; init; } = GitHubClientAuthenticationMode.Token;
    public BasicAuthObj? BasicAuth { get; init; }
    public TokenAuthObj? TokenAuth { get; init; }
    
    public record BasicAuthObj
    {
        public string Login { get; init; } = "";
        public string Password { get; init; } = "";
    }

    public record TokenAuthObj
    {
        public string ApiToken { get; init; } = "";
    }
}
