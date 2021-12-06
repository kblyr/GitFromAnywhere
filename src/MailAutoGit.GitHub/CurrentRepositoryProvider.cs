namespace MailAutoGit;

sealed class CurrentRepositoryProvider
{
    readonly ILogger<CurrentRepositoryProvider> _logger;
    readonly IOptions<CurrentRepositoryOptions> _options;
    readonly GitHubClient _client;

    Repository? _repository;

    public CurrentRepositoryProvider(ILogger<CurrentRepositoryProvider> logger, IOptions<CurrentRepositoryOptions> options, GitHubClient client)
    {
        _logger = logger;
        _options = options;
        _client = client;
    }

    public async Task<Repository> GetRepositoryAsync()
    {
        _logger.LogDebug("Getting current repository");

        if (_repository is not null)
        {
            _logger.LogDebug("Returning cached current repository");
            return _repository;
        }

        try 
        {
            var options = _options.Value;
            _logger.LogDebug("Getting current repository and caching");
            return _repository ??= await _client.Repository.Get(options.Owner, options.Name).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current repository from github.com");
            throw;
        }
    }
}
