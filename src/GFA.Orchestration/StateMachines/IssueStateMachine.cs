namespace GFA.StateMachines;

sealed class IssueStateMachine : MassTransitStateMachine<IssueInstance>
{
    readonly ILogger<IssueStateMachine> _logger;

    public IssueStateMachine(ILogger<IssueStateMachine> logger)
    {
        _logger = logger;
        ConfigureEvents();
        ConfigureEventActivities();

        InstanceState(instance => instance.CurrentState);
    }

    public State Created { get; private set; } = default!;

    public Event<IssueCreated> OnIssueCreated { get; private set; } = default!;
    public Event<CreatedIssueEmailSentToSubscriber> OnCreatedIssueEmailSentToSubscriber { get; private set; } = default!;

    void ConfigureEvents()
    {
        Event(() => OnIssueCreated, 
            correlation => {
                correlation.CorrelateBy((instance, context) => 
                    instance.Issue.RepositoryId == context.Message.RepositoryId &&
                    instance.Issue.Number == context.Message.Number
                ).SelectId(context => NewId.NextGuid());
            }
        );

        Event(() => OnCreatedIssueEmailSentToSubscriber,
            correlation => {
                correlation.CorrelateBy((instance, context) =>
                    instance.Issue.RepositoryId == context.Message.RepositoryId &&
                    instance.Issue.Number == context.Message.Number);
            }
        );
    }

    void ConfigureEventActivities()
    {
        Initially(
            When(OnIssueCreated)
                .Then(context => {
                    _logger.LogInformation("Issue created | Repository ID: {repositoryId}, Number: {number}, Title: {title}", context.Data.RepositoryId, context.Data.Number, context.Data.Title);
                    context.Instance.Issue = new()
                    {
                        RepositoryId = context.Data.RepositoryId,
                        Number = context.Data.Number,
                        SubscriberEmailAddress = context.Data.SubscriberEmailAddress,
                        IsSubscriberNotified = false
                    };
                })
                .Send(context => new SendCreatedIssueEmailToSubscriber() {
                    RepositoryId = context.Data.RepositoryId,
                    Number = context.Data.Number,
                    Title = context.Data.Title,
                    EmailAddress = context.Data.SubscriberEmailAddress
                })
                .TransitionTo(Created)
        );

        During(Created,
            When(OnCreatedIssueEmailSentToSubscriber)
                .Then(context => {
                    _logger.LogInformation("Subscriber notified | Repository ID: {repositoryId}, Number: {number}", context.Data.RepositoryId, context.Data.Number);
                    context.Instance.Issue.IsSubscriberNotified = true;
                })
        );
    }
}

public record IssueInstance : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";

    public IssueObj Issue { get; set; } = default!;

    public record IssueObj
    {
        public long RepositoryId { get; set; }
        public int Number { get; set; }
        public string SubscriberEmailAddress { get; set; } = "";
        public bool IsSubscriberNotified { get; set; }
        public bool IsOpen { get; set; }
    }
}

