namespace MailAutoGit.StateMachines;

sealed class FeatureRequestStateMachine : MassTransitStateMachine<FeatureRequestInstance>
{
    readonly ILogger<FeatureRequestStateMachine> _logger;

    public FeatureRequestStateMachine(ILogger<FeatureRequestStateMachine> logger)
    {
        _logger = logger;
        ConfigureEvents();
        ConfigureEventActivities();

        InstanceState(instance => instance.CurrentState);
    }

    public State Created { get; private set; } = default!;

    public Event<FeatureRequestCreated> OnFeatureRequestCreated { get; private set; } = default!;
    public Event<CreatedFeatureRequestEmailSentToSubscriber> OnCreatedFeatureRequestEmailSentToSubscriber { get; private set; } = default!;

    void ConfigureEvents()
    {
        Event(() => OnFeatureRequestCreated, 
            correlation => {
                correlation.CorrelateBy((instance, context) => 
                    instance.FeatureRequest.RepositoryId == context.Message.RepositoryId &&
                    instance.FeatureRequest.Number == context.Message.Number
                ).SelectId(context => NewId.NextGuid());
            }
        );

        Event(() => OnCreatedFeatureRequestEmailSentToSubscriber,
            correlation => {
                correlation.CorrelateBy((instance, context) =>
                    instance.FeatureRequest.RepositoryId == context.Message.RepositoryId &&
                    instance.FeatureRequest.Number == context.Message.Number);
            }
        );
    }

    void ConfigureEventActivities()
    {
        Initially(
            When(OnFeatureRequestCreated)
                .Then(context => {
                    _logger.LogInformation("Feature request created | Repository ID: {repositoryId}, Number: {number}, Title: {title}", context.Data.RepositoryId, context.Data.Number, context.Data.Title);
                    context.Instance.FeatureRequest = new()
                    {
                        RepositoryId = context.Data.RepositoryId,
                        Number = context.Data.Number,
                        SubscriberEmailAddress = context.Data.SubscriberEmailAddress,
                        IsSubscriberNotified = false
                    };
                })
                .Send(context => new SendCreatedFeatureRequestEmailToSubscriber() {
                    RepositoryId = context.Data.RepositoryId,
                    Number = context.Data.Number,
                    Title = context.Data.Title,
                    EmailAddress = context.Data.SubscriberEmailAddress
                })
                .TransitionTo(Created)
        );

        During(Created,
            When(OnCreatedFeatureRequestEmailSentToSubscriber)
                .Then(context => {
                    _logger.LogInformation("Subscriber notified | Repository ID: {repositoryId}, Number: {number}", context.Data.RepositoryId, context.Data.Number);
                    context.Instance.FeatureRequest.IsSubscriberNotified = true;
                })
        );
    }
}

public record FeatureRequestInstance : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = "";

    public FeatureRequestObj FeatureRequest { get; set; } = default!;

    public record FeatureRequestObj
    {
        public long RepositoryId { get; set; }
        public int Number { get; set; }
        public string SubscriberEmailAddress { get; set; } = "";
        public bool IsSubscriberNotified { get; set; }
        public bool IsOpen { get; set; }
    }
}

