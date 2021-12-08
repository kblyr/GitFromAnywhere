namespace GFA;

sealed class GitHubClientFactory
{
    readonly ILogger<GitHubClientFactory> _logger;
    readonly IOptions<GitHubClientOptions> _options;

    public GitHubClientFactory(ILogger<GitHubClientFactory> logger, IOptions<GitHubClientOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public GitHubClient CreateClient()
    {
        var options = _options.Value;
        var client = new GitHubClient(new ProductHeaderValue(options.ProductHeader));

        switch (options.AuthenticationMode)
        {
            case GitHubClientAuthenticationMode.Basic:
                if (options.BasicAuth is null)
                    throw new GFAException("Basic authentication requires login and password which are not provided");

                if (string.IsNullOrWhiteSpace(options.BasicAuth.Login))
                    throw new GFAException("Login is required but is either null or white-space");

                if (string.IsNullOrWhiteSpace(options.BasicAuth.Password))
                    throw new GFAException("Password is required but is either null or white-space");

                client.Credentials = new Credentials(options.BasicAuth.Login, options.BasicAuth.Password);
                _logger.LogDebug("Authenticating GitHub Client using login and password");
                break;
            case GitHubClientAuthenticationMode.Token:
                if (options.TokenAuth is null || string.IsNullOrWhiteSpace(options.TokenAuth.ApiToken))
                    throw new GFAException("Token authentication requires api token");

                client.Credentials = new Credentials(options.TokenAuth.ApiToken);
                _logger.LogDebug("Authenticating GitHub Client using token");
                break;
            case GitHubClientAuthenticationMode.None:
                _logger.LogDebug("GitHub Client is unauthenticated");
                break;
            default:
                throw new GFAException($"GitHub authentication mode '{options.AuthenticationMode.ConvertToString()}' is unsupported");
        }

        return client;
    }
}
