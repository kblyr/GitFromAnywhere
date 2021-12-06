namespace MailAutoGit;

public static class GitHubClientAuthenticationModeExtensions
{
    public static string ConvertToString(this GitHubClientAuthenticationMode mode) => mode switch
    {
        GitHubClientAuthenticationMode.None => "None",
        GitHubClientAuthenticationMode.Basic => "Basic",
        GitHubClientAuthenticationMode.Token => "Token",
        _ => mode.ToString()
    };
}