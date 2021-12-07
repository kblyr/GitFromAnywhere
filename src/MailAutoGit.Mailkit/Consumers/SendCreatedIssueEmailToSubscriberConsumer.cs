namespace MailAutoGit.Consumers;

sealed class SendCreatedIssueEmailToSubscriberConsumer : IConsumer<SendCreatedIssueEmailToSubscriber>
{
    public Task Consume(ConsumeContext<SendCreatedIssueEmailToSubscriber> context)
    {
        throw new NotImplementedException();
    }
}