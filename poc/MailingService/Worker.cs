using System.Text.Json;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Options;

namespace POC.MailingService;

public class Worker : BackgroundService
{
    readonly ILogger<Worker> _logger;
    readonly IOptions<ImapSettingsConfig> _config;

    public Worker(ILogger<Worker> logger, IOptions<ImapSettingsConfig> config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = _config.Value;

        using var client = new ImapClient();
        await client.ConnectAsync(config.Server, config.Port, config.Ssl, stoppingToken);
        client.AuthenticationMechanisms.Remove("XOAUTH2");
        await client.AuthenticateAsync(config.Login, config.Password, stoppingToken);
        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite);

        while (!stoppingToken.IsCancellationRequested)
        {
            await inbox.StatusAsync(StatusItems.Unread, stoppingToken);
            var unreadMessagesCount = inbox.Unread;

            if (unreadMessagesCount > 0)
            {
                _logger.LogInformation("There are {count} unread messages", unreadMessagesCount);
                var unreadMessageIds = await inbox.SearchAsync(SearchQuery.NotSeen, stoppingToken);

                for (int index = 0; index < unreadMessageIds.Count; index++)
                {
                    var message = await inbox.GetMessageAsync(unreadMessageIds[index], stoppingToken);
                    var headers = message.Headers;
                    var from = headers.Single(header => header.Field == "From");
                    bool.TryParse(headers.SingleOrDefault(header => header.Field == "POC-Mailing__IsFeatureRequest")?.Value, out bool isFeatureRequest);

                    _logger.LogInformation("Message ID: {messageId}", message.MessageId);
                    _logger.LogInformation("Sender: {sender}", from.Value);
                    _logger.LogInformation("Is Feature Request: {isFeatureRequest}", isFeatureRequest);

                    if (isFeatureRequest)
                    {
                       try  
                       {
                            var featureRequest = JsonSerializer.Deserialize<FeatureRequest>(message.TextBody);

                            if (featureRequest is not null)
                            {
                                _logger.LogInformation("Feature Request, First Name: {firstName}, Last Name: {lastName}, Date of Birth: {birthDate:yyyy-MM-dd}, Age: {age}", featureRequest.FirstName, featureRequest.LastName, featureRequest.BirthDate, featureRequest.Age);
                            }
                       }
                       catch (JsonException ex)
                       {
                           _logger.LogError(ex, "Failed to deserialize content: {content}", message.TextBody);
                       }
                    }

                    await inbox.AddFlagsAsync(unreadMessageIds[index], MessageFlags.Seen, true, stoppingToken);
                }
            }
            else
            {
                _logger.LogInformation("No unread messages");
            }

            await Task.Delay(10_000, stoppingToken);
        }

        await client.DisconnectAsync(true, stoppingToken);
    }
}

public record ImapSettingsConfig
{
    public const string ConfigKey = "ImapSettings";

    public string Server { get; init; } = "";
    public int Port { get; init; }
    public bool Ssl { get; init; }
    public string Login { get; init; } = "";
    public string Password { get; init; } = "";
}

public record FeatureRequest
{
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public DateTime BirthDate { get; init; }
    public int Age { get; init; }
}