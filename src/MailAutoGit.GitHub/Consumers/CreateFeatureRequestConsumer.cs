
using MailAutoGit.Events;

namespace MailAutoGit.Consumers;

sealed class CreateFeatureRequestConsumer : IConsumer<CreateFeatureRequest>
{
    readonly ILogger<CreateFeatureRequestConsumer> _logger;
    readonly GitHubClientFactory _clientFactory;

    public CreateFeatureRequestConsumer(ILogger<CreateFeatureRequestConsumer> logger, GitHubClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task Consume(ConsumeContext<CreateFeatureRequest> context)
    {
        var client = _clientFactory.CreateClient();

        var newFeatureRequest = new NewIssue(context.Message.Title)
        {
            Body = context.Message.Description,
        };

        Issue? createdFeatureRequest = null;

        try 
        {
            createdFeatureRequest = await client.Issue.Create(context.Message.RepositoryId, new NewIssue(context.Message.Title)
            {
                Body = context.Message.Description
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create feature request");
            throw;
        }

        if (createdFeatureRequest is null)
            return;

        try 
        {
            _logger.LogDebug("Publishing event: {event}", nameof(FeatureRequestCreated));
            await context.Publish<FeatureRequestCreated>(new()
            {
                RepositoryId = createdFeatureRequest.Repository.Id,
                Number = createdFeatureRequest.Number,
                Title = createdFeatureRequest.Title,
                SubscriberEmailAddress = context.Message.SubscriberEmailAddress
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {event}", nameof(FeatureRequestCreated));
            throw;
        }
    }
}
