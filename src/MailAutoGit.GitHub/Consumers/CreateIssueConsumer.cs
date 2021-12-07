
using MailAutoGit.Events;

namespace MailAutoGit.Consumers;

sealed class CreateIssueConsumer : IConsumer<CreateIssue>
{
    readonly ILogger<CreateIssueConsumer> _logger;
    readonly GitHubClientFactory _clientFactory;

    public CreateIssueConsumer(ILogger<CreateIssueConsumer> logger, GitHubClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task Consume(ConsumeContext<CreateIssue> context)
    {
        var client = _clientFactory.CreateClient();

        var newFeatureRequest = new NewIssue(context.Message.Title)
        {
            Body = context.Message.Description,
        };

        Issue? createdIssue = null;

        try 
        {
            createdIssue = await client.Issue.Create(context.Message.RepositoryId, new NewIssue(context.Message.Title)
            {
                Body = context.Message.Description
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create issue");
            throw;
        }

        if (createdIssue is null)
            return;

        IssueCreated @event = new()
        {
            RepositoryId = createdIssue.Repository.Id,
            Number = createdIssue.Number,
            Title = createdIssue.Title,
            SubscriberEmailAddress = context.Message.SubscriberEmailAddress,
            IsOpen = createdIssue.State == ItemState.Open
        };

        try 
        {
            _logger.LogDebug("Publishing event: {event} | Repository ID: {repositoryId}, Number: {number}, Title: {title}", 
                nameof(IssueCreated), 
                @event.RepositoryId, 
                @event.Number, 
                @event.Title);
            await context.Publish(@event).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {event} | Repository ID: {repositoryId}, Number: {number}, Title: {title}", 
                nameof(IssueCreated), 
                @event.RepositoryId, 
                @event.Number, 
                @event.Title);
            throw;
        }
    }
}
