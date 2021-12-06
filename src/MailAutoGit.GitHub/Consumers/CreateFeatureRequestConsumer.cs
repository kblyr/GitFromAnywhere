
using MailAutoGit.Events;

namespace MailAutoGit.Consumers;

sealed class CreateFeatureRequestConsumer : IConsumer<CreateFeatureRequest>
{
    readonly ILogger<CreateFeatureRequestConsumer> _logger;
    readonly CurrentRepositoryProvider _currentRepositoryProvider;
    readonly GitHubClientFactory _clientFactory;

    public CreateFeatureRequestConsumer(ILogger<CreateFeatureRequestConsumer> logger, CurrentRepositoryProvider currentRepositoryProvider, GitHubClientFactory clientFactory)
    {
        _logger = logger;
        _currentRepositoryProvider = currentRepositoryProvider;
        _clientFactory = clientFactory;
    }

    public async Task Consume(ConsumeContext<CreateFeatureRequest> context)
    {
        var client = _clientFactory.CreateClient();
        var repository = await _currentRepositoryProvider.GetRepositoryAsync().ConfigureAwait(false);

        var newFeatureRequest = new NewIssue(context.Message.Title)
        {
            Body = context.Message.Description,
        };

        Issue? createdFeatureRequest = null;

        try 
        {
            createdFeatureRequest = await client.Issue.Create(repository.Id, new NewIssue(context.Message.Title)
            {
                Body = context.Message.Description
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {

        }

        if (createdFeatureRequest is null)
            return;

        try 
        {
            _logger.LogDebug("Publishing event: {event}", nameof(FeatureRequestCreated));
            await context.Publish<FeatureRequestCreated>(new()
            {
                Number = createdFeatureRequest.Number,
                Title = createdFeatureRequest.Title
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {event}", nameof(FeatureRequestCreated));
            throw;
        }
    }
}
