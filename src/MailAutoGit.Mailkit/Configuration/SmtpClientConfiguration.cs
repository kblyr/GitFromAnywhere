using Microsoft.Extensions.DependencyInjection;

namespace MailAutoGit.Configuraiton;

public record SmtpClientConfiguration
{
    public string Server { get; init; } = "";
    public int Port { get; init; }
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
}

public static class IServiceCollectionExtensions
{
}

public class SmtpClientConfigurationMapper
{

}